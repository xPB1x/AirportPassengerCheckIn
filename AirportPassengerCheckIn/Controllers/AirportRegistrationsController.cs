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
        public async Task<IActionResult> BookingSearch()
        {
            string? passportNumber = TempData["SearchPassport"] as string;
            if (!string.IsNullOrWhiteSpace(passportNumber))
            {
                var model = await GetBookingSearchResults(passportNumber);
                return View("BookingSearchResults", model);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookingSearchResults(string passportNumber)
        {
            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                ModelState.AddModelError(nameof(passportNumber), "Введите номер пасспорта");
                return View(new AirportBookingViewModel());
            }

            var model = await GetBookingSearchResults(passportNumber);
            return View(model);
        }

        private async Task<AirportBookingViewModel> GetBookingSearchResults(
            string passportNumber)
        {
            passportNumber = passportNumber.Trim().ToUpper();

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

            return new AirportBookingViewModel
            {
                PassportNumber = passportNumber,
                Bookings = bookings
            };
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

            return RedirectToAction(nameof(SelectSeat), new {id = bookingPassenger.Id});

        }

        [HttpGet]
        public async Task<IActionResult> SelectSeat(int id)
        {
            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.Aircraft)
                            .ThenInclude(a => a.Seats)
                .Include(bp => bp.Passenger)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (booking.Status != BookingStatus.Confirmed ||
                flight.DepartureTime <= now ||
                flight.DepartureTime > now.AddHours(24) ||
                registration == null ||
                registration.Status != RegistrationStatus.Active)
            {
                TempData["ErrorMessage"] = "You can't continue registration";
                return RedirectToAction(nameof(BookingSearch));
            }

            var occupiedSeatIds = await _context.Registrations
                .Where(r =>
                    r.BookingPassenger.Booking.Flight.Id == flight.Id &&
                    r.SeatId != null &&
                    r.Status != RegistrationStatus.Canceled &&
                    r.BookingPassengerId != bookingPassenger.Id)
                .Select(r => r.SeatId!.Value)
                .ToListAsync();

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(flight.DepartureAirport.TimeZoneId);
            var localDepartureTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(flight.DepartureTime, DateTimeKind.Utc),
                timeZone);

            var model = new SelectSeatViewModel
            {
                BookingPassengerId = bookingPassenger.Id,
                BookingNumber = booking.BookingNumber,
                FlightNumber = flight.FlightNumber,

                PassengerName = string.Join(" ", new[]
                    {
                        bookingPassenger.Passenger.LastName,
                        bookingPassenger.Passenger.FirstName,
                        bookingPassenger.Passenger.MiddleName
                    }.Where(s => !string.IsNullOrWhiteSpace(s))),

                DepartureAirport =
                    $"{flight.DepartureAirport.City} ({flight.DepartureAirport.CodeIATA})",

                ArrivalAirport =
                    $"{flight.ArrivalAirport.City} ({flight.ArrivalAirport.CodeIATA})",

                DepartureTime = localDepartureTime,

                SelectedSeatId = registration.SeatId,

                Seats = flight.Aircraft.Seats
                    .OrderBy(s => s.SeatNumber)
                    .Select(s => new SeatViewModel
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        Class = s.Class,
                        IsOccupied = occupiedSeatIds.Contains(s.Id)
                    })
                    .ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmSelectSeat(
            int bookingPassengerId,
            int? selectedSeatId)
        {
            if (selectedSeatId == null)
            {
                TempData["ErrorMessage"] = "You have to choose the seat";
                return RedirectToAction(nameof(SelectSeat), new { id = bookingPassengerId });
            }

            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Registration)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (booking.Status != BookingStatus.Confirmed ||
                flight.DepartureTime <= now ||
                flight.DepartureTime > now.AddHours(24) ||
                registration == null ||
                registration.Status != RegistrationStatus.Active)
            {
                TempData["ErrorMessage"] = "You can't continue registration";
                return RedirectToAction(nameof(BookingSearch));
            }

            var seat = await _context.Seats.FirstOrDefaultAsync(s => s.Id == selectedSeatId);
            if (seat == null || seat.AircraftId != flight.AircraftId)
            {
                TempData["ErrorMessage"] = "This seat is not available";
                return RedirectToAction(nameof(SelectSeat), new {id=bookingPassengerId});
            }

            var isOccupied = await _context.Registrations
                .AnyAsync(r =>
                    r.BookingPassenger.Booking.Flight.Id == flight.Id &&
                    r.BookingPassengerId != bookingPassengerId &&
                    r.SeatId == seat.Id &&
                    (r.Status == RegistrationStatus.Active || r.Status == RegistrationStatus.Finished));

            if (isOccupied)
            {
                TempData["ErrorMessaga"] = "This seat is already occupied";
                return RedirectToAction(nameof(SelectSeat), new {id=bookingPassengerId});
            }

            registration.SeatId = seat.Id;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"The seat {seat.SeatNumber} has been choosen completly";
            return RedirectToAction(nameof(ConfirmRegistration), new { id = bookingPassengerId });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmRegistration(int id)
        {
            var now = DateTime.UtcNow;
            var bookingPassenger = await _context.BookingPassengers
            .Include(bp => bp.Booking)
                .ThenInclude(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
            .Include(bp => bp.Booking)
                .ThenInclude(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
            .Include(bp => bp.Registration)
                .ThenInclude(r => r!.Seat)
            .Include(bp => bp.Passenger)
            .FirstOrDefaultAsync(bp => bp.Id == id);

            if (bookingPassenger == null)
                return NotFound();

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (booking.Status != BookingStatus.Confirmed ||
                registration == null ||
                registration.Type != RegistrationType.Airport ||
                registration.Status != RegistrationStatus.Active ||
                registration.SeatId == null ||
                flight.DepartureTime <= now ||
                flight.DepartureTime > now.AddHours(24))
            {
                TempData["ErrorMessage"] = "Can't confirm registration";
                return RedirectToAction(nameof(BookingSearch));
            }

            var departureTime = DateTime.SpecifyKind(
                flight.DepartureTime,
                DateTimeKind.Utc);

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                flight.DepartureAirport.TimeZoneId);

            departureTime = TimeZoneInfo.ConvertTimeFromUtc(
                departureTime,
                timeZone);

            var model = new ConfirmViewModel
            {
                BookingPassengerId = bookingPassenger.Id,
                BookingNumber = booking.BookingNumber,

                PassengerName = string.Join(" ", new[]
                {
                    bookingPassenger.Passenger.LastName,
                    bookingPassenger.Passenger.FirstName,
                    bookingPassenger.Passenger.MiddleName
                }.Where(s => !string.IsNullOrWhiteSpace(s))),

                PassengerPassport = bookingPassenger.Passenger.PassportNumber,
                FlightNumber = flight.FlightNumber,

                DepartureAirport =
                    $"{flight.DepartureAirport.City} ({flight.DepartureAirport.CodeIATA})",

                ArrivalAirport =
                    $"{flight.ArrivalAirport.City} ({flight.ArrivalAirport.CodeIATA})", 

                DepartureTime = departureTime,
                SeatNumber = registration.Seat!.SeatNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int bookingPassengerId)
        {
            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                    .ThenInclude(r => r!.Seat)
                .Include(bp => bp.Passenger)
                .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (booking.Status != BookingStatus.Confirmed ||
                registration == null ||
                registration.Type != RegistrationType.Airport ||
                registration.Status != RegistrationStatus.Active ||
                registration.SeatId == null ||
                flight.DepartureTime <= now ||
                flight.DepartureTime > now.AddHours(24))
            {
                TempData["ErrorMessage"] = "Can't finish the registration";
                return RedirectToAction(nameof(BookingSearch));
            }

            var seat = await _context.Seats
                .FirstOrDefaultAsync(s => s.Id == registration.SeatId);

            if (seat == null || seat.AircraftId != flight.AircraftId)
            {
                TempData["ErrorMessage"] = "Seat is not available";
                return RedirectToAction(nameof(SelectSeat), new {id=bookingPassengerId});
            }

            var isOccupied = await _context.Registrations
                .AnyAsync(r =>
                    r.BookingPassenger.Booking.FlightId == flight.Id &&
                    r.BookingPassengerId != bookingPassengerId &&
                    r.SeatId == registration.SeatId &&
                    (r.Status == RegistrationStatus.Active ||
                     r.Status == RegistrationStatus.Finished));

            if (isOccupied)
            {
                TempData["ErrorMessage"] =
                    "This seat is already occupied";

                return RedirectToAction(
                    nameof(SelectSeat),
                    new { id = bookingPassengerId });
            }

            registration.Status = RegistrationStatus.Finished;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registartion has been completed succesfully";

            TempData["SearchPassport"] = bookingPassenger.Passenger.PassportNumber;

            return RedirectToAction(nameof(BookingSearch));
        }
    }
}
