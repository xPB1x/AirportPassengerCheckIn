using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs.ViewModels.Registrations;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Services.Registration.Interfaces;
using AirportPassengerCheckIn.Services.Registration.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirportPassengerCheckIn.Controllers
{
    [Authorize(Roles = ("Admin,AirportEmployee"))]
    public class AirportRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAirportRegistrationService _registrationService;
        public AirportRegistrationController (
            ApplicationDbContext context,
            IAirportRegistrationService registrationService)
        {
            _context = context;
            _registrationService = registrationService;
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
                        RegistrationType = bp.Registration == null
                            ? null
                            : bp.Registration.Type.ToString(),

                        SeatNumber = bp.Registration == null ||
                                     bp.Registration.Seat == null
                            ? null
                            : bp.Registration.Seat.SeatNumber,
                        CountPaidBaggage = bp.CountPaidBaggages,
                        RegisteredBaggageCount = bp.Registration == null ? 0 : bp.Registration.Baggages.Count()
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _registrationService.StartRegistrationAsync(
                userId!, bookingPassengerId);

            if (result.Error == RegistrationError.NotFound)
            {
                return NotFound();
            }

            if (result.Error == RegistrationError.RegistrationAlreadyActive)
                return RedirectToAction(nameof(ContinueAirport), new { bookingPassengerId });

            if (!result.Succeeded)
            {
                var bookingPassenger = await _context.BookingPassengers
                    .Include(bp => bp.Passenger)
                    .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

                TempData["ErrorMessage"] = result.Error.ToString();
                TempData["SearchPassport"] = bookingPassenger!.Passenger.PassportNumber;
                return RedirectToAction(nameof(BookingSearch));
            }


            return RedirectToAction(nameof(SelectSeat), new {id = bookingPassengerId});
        }

        [HttpGet]
        public async Task<IActionResult> ContinueAirport(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _registrationService.GetContinueStepAsync(userId!, id);

            if (result.Error == RegistrationError.NotFound)
            {
                return NotFound();
            }
            if (!result.Succeeded)
            {
                var bookingPassenger = await _context.BookingPassengers
                    .Include(bp => bp.Passenger)
                    .FirstOrDefaultAsync(bp => bp.Id == id);

                TempData["ErrorMessage"] = result.Error.ToString();
                TempData["SearchPassport"] = bookingPassenger!.Passenger.PassportNumber;
                return RedirectToAction(nameof(BookingSearch));
            }

            if (result.NextStep == RegistrationStep.SelectSeat)
            {
                return RedirectToAction(nameof(SelectSeat), new { id });
            }

            return RedirectToAction(nameof(ConfirmRegistration), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeat(int id)
        {
            var now = DateTime.UtcNow;
            var result = await _registrationService.GetAvailableSeatsAsync(id);

            if (result.Error == RegistrationError.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                var bookingPassengerPassport = await _context.BookingPassengers
                    .Include(bp => bp.Passenger)
                    .FirstOrDefaultAsync(bp => bp.Id == id);

                TempData["ErrorMessage"] = result.Error.ToString();
                TempData["SearchPassport"] = bookingPassengerPassport!.Passenger.PassportNumber;
                return RedirectToAction(nameof(BookingSearch));
            }

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

            

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

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

                SelectedSeatId = registration!.SeatId,

                Seats = result.Seats
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
            if (!selectedSeatId.HasValue)
            {
                return RedirectToAction(nameof(SelectSeat),
                    new { id = bookingPassengerId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _registrationService.SelectSeatAsync(bookingPassengerId, selectedSeatId.Value);

            if (result.Error == RegistrationError.NotFound)
                return NotFound();

            if (result.Error == RegistrationError.SeatOccupied)
            {
                TempData["ErrorMessage"] = "This seat has been occupied";

                return RedirectToAction(
                    nameof(SelectSeat),
                    new { id = bookingPassengerId });
            }

            if (!result.Succeeded)
            {
                var bookingPassengerPassport = await _context.BookingPassengers
                    .Include(bp => bp.Passenger)
                    .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

                TempData["ErrorMessage"] = result.Error.ToString();
                TempData["SearchPassport"] = bookingPassengerPassport!.Passenger.PassportNumber;
                return RedirectToAction(nameof(BookingSearch));
            }


            TempData["SuccessMessage"] = "Seat was succesfully selected";

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var result = await _registrationService.FinishRegistrationAsync(bookingPassengerId);

            if (result.Error == RegistrationError.NotFound)
                return NotFound();

            if (result.Error == RegistrationError.SeatNotSelected)
            {
                return RedirectToAction(
                    nameof(SelectSeat),
                    new { id = bookingPassengerId });
            }

            var bookingPassengerPassport = await _context.BookingPassengers
                    .Include(bp => bp.Passenger)
                    .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            TempData["SearchPassport"] = bookingPassengerPassport!.Passenger.PassportNumber;

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error.ToString();
                return RedirectToAction(nameof(BookingSearch));
            }

            TempData["SuccessMessage"] = "Registartion has been completed succesfully";
            return RedirectToAction(nameof(BookingSearch));
        }

        [HttpGet]
        public async Task<IActionResult> ManageBaggage(int id)
        {
            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Passenger)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                    .ThenInclude(r => r!.Baggages)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var passenger = bookingPassenger.Passenger;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (booking.Status != BookingStatus.Confirmed ||
                flight.DepartureTime <= now || flight.DepartureTime > now.AddHours(24) ||
                registration == null ||
                registration.Status != RegistrationStatus.Finished)
            {
                TempData["ErrorMessage"] = "Baggage registration is not available";
                return RedirectToAction(nameof(BookingSearch));
            }

            var model = new BaggageViewModel
            {
                BookingPassengerId = bookingPassenger.Id,
                PassengerName = bookingPassenger.Passenger.LastName + " " +
                    bookingPassenger.Passenger.FirstName + " " +
                    (bookingPassenger.Passenger.MiddleName ?? ""),
                BookingNumber = booking.BookingNumber,
                FlightNumber = flight.FlightNumber,
                CountPaidBaggage = bookingPassenger.CountPaidBaggages,
                MaxBaggageWeight = flight.MaxBaggageWeight,
                Baggages = registration.Baggages
                    .Select(b => new BaggageItemViewModel
                    {
                        Id = b.Id,
                        Weight = b.Weight,
                        IsOversized = b.IsOversized
                    })
                    .ToList()
            };
            TempData["SearchPassport"] = bookingPassenger.Passenger.PassportNumber;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBaggage(BaggageViewModel model)
        {
            var now = DateTime.UtcNow;
            var result = await _registrationService.AddBaggage(model.BookingPassengerId, model);

            if (result.Error == RegistrationError.NotFound)
                return NotFound();

            var bookingPassengerPassport = await _context.BookingPassengers
                    .Include(bp => bp.Passenger)
                    .FirstOrDefaultAsync(bp => bp.Id == model.BookingPassengerId);

            TempData["SearchPassport"] = bookingPassengerPassport!.Passenger.PassportNumber;

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error.ToString();
                return RedirectToAction(nameof(BookingSearch));
            }


            TempData["SuccessMessage"] = "Baggage has been registered successfully.";

            return RedirectToAction(nameof(ManageBaggage),
                new { id = model.BookingPassengerId });
        }
    }
}
