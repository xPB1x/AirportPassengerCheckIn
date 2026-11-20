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
using AirportPassengerCheckIn.Services.Registration.Interfaces;
using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Services.Registration;


namespace AirportPassengerCheckIn.Controllers
{
    [Authorize(Roles = "Passenger")]
    public class OnlineRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOnlineRegistrationService _registrationService;

        public OnlineRegistrationController(
            ApplicationDbContext context, IOnlineRegistrationService registrationService)
        {
            _context = context;
            _registrationService = registrationService;
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

        [HttpGet]
        public async Task<IActionResult> StartOnline(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _registrationService.CheckStartRegistrationAsync(userId!, id);
            if (result.Error == RegistrationError.NotFound)
            {
                return NotFound();
            }
            if (result.Error == RegistrationError.RegistrationAlreadyActive)
            {
                return RedirectToAction(nameof(ContinueOnline), new { id });
            }
            if (!result.Succeeded)
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

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

            var booking = bookingPassenger!.Booking;
            var flight = booking.Flight;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmStartOnline(int bookingPassengerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _registrationService.StartRegistrationAsync(userId!, bookingPassengerId);

            if (result.Error == RegistrationError.NotFound)
            {
                return NotFound();
            }

            if (result.Error == RegistrationError.RegistrationAlreadyActive)
                return RedirectToAction(nameof(ContinueOnline), new { bookingPassengerId });

            if (!result.Succeeded)
                return RedirectToAction(nameof(OnlineBookings));

            return RedirectToAction(nameof(SelectSeat), new {id = bookingPassengerId});
        }

        [HttpGet]
        public async Task<IActionResult> ContinueOnline(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _registrationService.GetContinueStepOnlineAsync(userId!, id);

            if (result.Error == RegistrationError.NotFound)
            {
                return NotFound();
            }
            if (!result.Succeeded)
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

            if (result.NextStep == RegistrationStep.SelectSeat)
            {
                return RedirectToAction(nameof(SelectSeat), new { id });
            }

            return RedirectToAction(nameof(ConfirmOnline), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeat(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _registrationService.GetAvailableSeatsAsync(userId!, id);

            if (result.Error == RegistrationError.NotFound)
                return NotFound();

            if (!result.Succeeded)
                return RedirectToAction(nameof(OnlineBookings));

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
                .FirstOrDefaultAsync(bp =>
                    bp.Id == id &&
                    bp.Booking.UserId == userId);


            var booking = bookingPassenger!.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;
            var now = DateTime.UtcNow;

            var occupiedSeatIds = await _context.Registrations
                .Where(r =>
                    r.BookingPassenger.Booking.Flight.Id == flight.Id &&
                    r.Status != RegistrationStatus.Canceled &&
                    r.SeatId != null &&
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
                        IsOccupied = s.IsOccupied
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

            var result = await _registrationService.SelectSeatAsync(userId!, bookingPassengerId, selectedSeatId.Value);

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
                return RedirectToAction(nameof(OnlineBookings));


            TempData["SuccessMessage"] = "Seat was succesfully selected";

            return RedirectToAction(nameof(ConfirmOnline), new { id = bookingPassengerId });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmOnline(int id)
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
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.Aircraft)
                .Include(bp => bp.Passenger)
                .Include(bp => bp.Registration)
                    .ThenInclude(r => r!.Seat)
                .FirstOrDefaultAsync(bp =>
                    bp.Id == id &&
                    bp.Booking.UserId == userId);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;


            if (booking.Status != BookingStatus.Confirmed ||
                registration == null ||
                registration.Status != RegistrationStatus.Active ||
                registration.SeatId == null ||
                flight.DepartureTime <= now ||
                flight.DepartureTime > now.AddHours(24))
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                flight.DepartureAirport.TimeZoneId);

            var localDepartureTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(flight.DepartureTime, DateTimeKind.Utc),
                timeZone);

            var model = new ConfirmViewModel
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
                PassengerPassport = bookingPassenger.Passenger.PassportNumber,

                DepartureAirport =
                    $"{flight.DepartureAirport.City} ({flight.DepartureAirport.CodeIATA})",

                ArrivalAirport =
                    $"{flight.ArrivalAirport.City} ({flight.ArrivalAirport.CodeIATA})",

                DepartureTime = localDepartureTime,
                SeatNumber = registration.Seat!.SeatNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FinishOnline(int bookingPassengerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var result = await _registrationService.FinishRegistrationAsync(userId!, bookingPassengerId);

            if (result.Error == RegistrationError.NotFound)
                return NotFound();

            if (result.Error == RegistrationError.SeatNotSelected)
            {
                return RedirectToAction(
                    nameof(SelectSeat),
                    new { id = bookingPassengerId });
            }

            if (!result.Succeeded)
                return RedirectToAction(nameof(OnlineBookings));

            TempData["SuccessMessage"] = "Online registration completed succesfully";

            return RedirectToAction(nameof(OnlineBookings));
        }
    }
}