using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs.ViewModels.Registrations;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirportPassengerCheckIn.Controllers
{
    [Authorize(Roles = ("Admin,AirportEmployee"))]
    public class AirportRegistrationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AirportRegistrationsController (ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult BookingSearch()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookingSearchResults(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                ModelState.AddModelError(
                    nameof(passportNumber),
                    "Введите номер паспорта.");

                return View(new AirportBookingViewModel());
            }

            passportNumber = passportNumber.Trim();
            var now = DateTime.UtcNow;

            var bookings = await _context.Bookings
            .Where(b =>
                b.Status == BookingStatus.Confirmed &&
                b.Flight.DepartureTime > now &&
                b.BookingPassengers.Any(bp =>
                    bp.Passenger.PassportNumber.ToUpper() == passportNumber))
            .SelectMany(b => b.BookingPassengers
                .Where(bp =>
                    bp.Passenger.PassportNumber.ToUpper() == passportNumber)
                .Select(bp => new AirportBookingItemViewModel
                {
                    BookingPassengerId = bp.Id,
                    BookingNumber = b.BookingNumber,
                    FlightNumber = b.Flight.FlightNumber,
                    DepartureAirport = b.Flight.DepartureAirport.CodeIATA,
                    ArrivalAirport = b.Flight.ArrivalAirport.CodeIATA,
                    DepartureTime = b.Flight.DepartureTime,
                    DepartureTimeZoneId = b.Flight.DepartureAirport.TimeZoneId,
                    PassengerName =
                        bp.Passenger.LastName + " " +
                        bp.Passenger.FirstName + " " +
                        (bp.Passenger.MiddleName ?? ""),
                    PassportNumber = bp.Passenger.PassportNumber,
                    RegistrationStatus = bp.Registration == null
                        ? null
                        : bp.Registration.Status.ToString(),
                    SeatNumber = bp.Registration == null ||
                                 bp.Registration.Seat == null
                        ? null
                        : bp.Registration.Seat.SeatNumber
                }))
            .OrderBy(b => b.DepartureTime)
            .ToListAsync();

            foreach (var booking in bookings)
            {
                var departureUtc = DateTime.SpecifyKind(
                    booking.DepartureTime,
                    DateTimeKind.Utc);

                if (!string.IsNullOrWhiteSpace(booking.DepartureTimeZoneId))
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                        booking.DepartureTimeZoneId);

                    booking.DepartureTime =
                        TimeZoneInfo.ConvertTimeFromUtc(departureUtc, timeZone);
                }
                else
                {
                    booking.DepartureTime = departureUtc;
                }
            }

            var model = new AirportBookingViewModel
            {
                PassportNumber = passportNumber,
                Bookings = bookings
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartRegistration(int bookingPassengerId)
        {
            var now = DateTime.UtcNow;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                TempData["ErrorMessage"] = "The booking is not confirmed";
                return RedirectToAction(nameof(BookingSearch));
            }

            var departureTime = bookingPassenger.Booking.Flight.DepartureTime;
            if (departureTime <= now || departureTime > now.AddHours(24))
            {
                TempData["ErrorMessage"] = "Registration is only available 24 hours before departure.";
                return RedirectToAction(nameof(BookingSearch));
            }

            if (bookingPassenger.Registration != null)
            {
                TempData["ErrorMessage"] = "Registraion is already exists";
                return RedirectToAction(nameof(BookingSearch));
            }

            var airportEmployeeId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.AirportEmployeeId)
                .FirstOrDefaultAsync();

            if (User.IsInRole("AirportEmployee") && airportEmployeeId == null)
                return Forbid();

            var registration = new Registration
            {
                RegistrationTime = now,
                Type = RegistrationType.Airport,
                Status = RegistrationStatus.Active,
                BookingPassengerId = bookingPassenger.Id,
                AirportEmployeeId = airportEmployeeId
            };

            _context.Registrations.Add(registration);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registation has started succesfully";

            return RedirectToAction(nameof(BookingSearch));

        }
    }
}
