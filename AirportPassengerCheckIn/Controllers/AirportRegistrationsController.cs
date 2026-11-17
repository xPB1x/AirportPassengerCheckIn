using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs.ViewModels.Registrations;
using AirportPassengerCheckIn.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Controllers
{
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
    }
}
