using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.DTOs.ViewModels;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.DTOs.ViewModels.Registrations;
using System.Diagnostics;


namespace AirportPassengerCheckIn.Controllers
{
    [Authorize(Roles = "Passenger")]
    public class RegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> OnlineBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var now = DateTime.UtcNow;

            var bookings = await _context.Bookings
                .Where(b =>
                    b.UserId == userId &&
                    b.Status == BookingStatus.Confirmed &&
                    b.Flight.DepartureTime > now)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.Passenger)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.Registration)
                .OrderBy(b => b.Flight.DepartureTime)
                .ToListAsync();

            var model = CreateBookingViewModel(bookings);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> StartOnline(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                .Include(bp => bp.Passenger)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp => bp.Booking.UserId == userId && bp.Id == id);

            Debug.WriteLine("before bp get");
            if (bookingPassenger == null)
            {
                return NotFound();
            }
            Debug.WriteLine("after bp get");

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;

            if (booking.Status != BookingStatus.Confirmed)
            {
                return NotFound();
            }
            Debug.WriteLine("after b.status");

            if (flight.DepartureTime <= now || flight.DepartureTime > now.AddHours(24))
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

            if (bookingPassenger.Registration != null)
            {
                //if (bookingPassenger.Registration.Status == RegistrationStatus.Active)
                //{
                //    return RedirectToAction(nameof(ContinueOnline), new { id });
                //}
                return RedirectToAction(nameof(OnlineBookings));
            }

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                flight.DepartureAirport.TimeZoneId);
            var localDepartureTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(flight.DepartureTime, DateTimeKind.Utc),
                timeZone);

            var model = new StartOnlineViewModel
            {
                BookingPassengerId = bookingPassenger.Id,
                BookingNumber = booking.BookingNumber,
                FlightNumber = flight.FlightNumber,
                PassengerFullName = string.Join(" ", new[]
                {
                    bookingPassenger.Passenger.LastName,
                    bookingPassenger.Passenger.FirstName,
                    bookingPassenger.Passenger.MiddleName
                }.Where(s => !string.IsNullOrWhiteSpace(s))),
                DepartureAirport =
                    $"{flight.DepartureAirport.City} ({flight.DepartureAirport.CodeIATA})",
                ArrivalAirport =
                    $"{flight.ArrivalAirport.City} ({flight.ArrivalAirport.CodeIATA})",
                DepartureTime = localDepartureTime
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ContinueOnline()
        {
            return null;
        }

        private List<OnlineBookingViewModel> CreateBookingViewModel(List<Booking> bookings)
        {
            var now = DateTime.UtcNow;
            return bookings.Select(b =>
            {
                var departureTimeZone =
                    TimeZoneInfo.FindSystemTimeZoneById(
                        b.Flight.DepartureAirport.TimeZoneId);
                var localDepartureTime = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(
                        b.Flight.DepartureTime,
                        DateTimeKind.Utc),
                    departureTimeZone);

                return new OnlineBookingViewModel
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber,
                    FlightNumber = b.Flight.FlightNumber,

                    DepartureAirport =
                        $"{b.Flight.DepartureAirport.City} ({b.Flight.DepartureAirport.CodeIATA})",

                    ArrivalAirport =
                        $"{b.Flight.ArrivalAirport.City} ({b.Flight.ArrivalAirport.CodeIATA})",

                    DepartureTime = localDepartureTime,

                    Passengers = b.BookingPassengers.Select(bp =>
                    {
                        var registration = bp.Registration;
                        bool canRegister =
                            b.Flight.DepartureTime > now &&
                            b.Flight.DepartureTime <= now.AddHours(24) &&
                            registration == null;

                        return new OnlineBookingPassengerViewModel
                        {
                            BookingPassengerId = bp.Id,
                            FullName = 
                                $"{bp.Passenger.LastName} " +
                                $"{bp.Passenger.FirstName} " +
                                $"{bp.Passenger.MiddleName}",
                            RegistrationStatus = registration?.Status,
                            CanRegister = canRegister,
                            HasActiveRegistration = registration?.Status == RegistrationStatus.Active,
                            IsRegistrationFinished = registration?.Status == RegistrationStatus.Finished
                        };
                    }).ToList(),
                };
            }).ToList();
        }
    }
}
