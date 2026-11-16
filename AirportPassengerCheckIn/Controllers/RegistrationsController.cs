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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmStartOnline(int bookingPassengerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp => bp.Booking.UserId == userId && bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;

            if (booking.Status != BookingStatus.Confirmed)
            {
                return RedirectToAction(nameof(OnlineBookings));
            }
            if (flight.DepartureTime <= now ||
                flight.DepartureTime > now.AddHours(24))
            {
                return RedirectToAction(nameof(OnlineBookings));
            }
            if (bookingPassenger.Registration != null)
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

            var registration = new Registration
            {
                BookingPassengerId = bookingPassenger.Id,
                RegistrationTime = now,
                Type = RegistrationType.Online,
                Status = RegistrationStatus.Active
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(SelectSeat), new {id = bookingPassenger.Id});
        }

        [HttpGet]
        public async Task<IActionResult> ContinueOnline(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Registration)
                .Include(bp => bp.Booking)
                    .ThenInclude(bp => bp.Flight)
                .FirstOrDefaultAsync(bp => bp.Booking.UserId == userId && bp.Id == id);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;


            if (booking.Status != BookingStatus.Confirmed ||
                flight.DepartureTime <= now || flight.DepartureTime > now.AddHours(24) ||
                registration == null ||
                registration.Type != RegistrationType.Online ||
                registration.Status != RegistrationStatus.Active)
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

            if (!registration.SeatId.HasValue)
            {
                return RedirectToAction(nameof(SelectSeat), new { id });
            }
            return RedirectToAction(nameof(ConfirmOnline), new { id });
        }



        [HttpGet]
        public async Task<IActionResult> SelectSeat(int id)
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
                            .ThenInclude(a => a.Seats)
                .Include(bp => bp.Passenger)
                .Include(bp => bp.Registration)
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
                flight.DepartureTime <= now ||
                flight.DepartureTime > now.AddHours(24))
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

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
            if (!selectedSeatId.HasValue)
            {
                return RedirectToAction(nameof(SelectSeat),
                    new { id = bookingPassengerId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp => bp.Booking.UserId == userId && bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (booking.Status != BookingStatus.Confirmed ||
                flight.DepartureTime <= now || flight.DepartureTime > now.AddHours(24) ||
                registration == null ||
                registration.Status != RegistrationStatus.Active)
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

            var seat = await _context.Seats.FirstOrDefaultAsync(s => s.Id == selectedSeatId.Value);
            if (seat == null)
            {
                return BadRequest("This seats doesn't exist");
            }

            var isOccupied = await _context.Registrations.AnyAsync(r =>
                r.SeatId == selectedSeatId.Value &&
                r.BookingPassenger.Booking.FlightId != flight.Id &&
                r.BookingPassengerId != bookingPassengerId &&
                (r.Status == RegistrationStatus.Active || r.Status == RegistrationStatus.Finished));

            if (isOccupied)
            {
                TempData["ErrorMessage"] = "This seat has been occupied";
                return RedirectToAction(nameof(SelectSeat), new { id = bookingPassengerId });
            }

            registration.Seat = seat;
            await _context.SaveChangesAsync();
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

            var model = new ConfirmOnlineViewModel
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
                SeatNumber = registration.Seat!.SeatNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FinishOnline(int bookingPassengerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Registration)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .FirstOrDefaultAsync(bp => bp.Booking.UserId == userId && bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return NotFound();
            }

            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (booking.Status != BookingStatus.Confirmed ||
                registration == null ||
                registration.Type != RegistrationType.Online ||
                registration.Status != RegistrationStatus.Active)
            {
                return RedirectToAction(nameof(OnlineBookings));
            }
            if (flight.DepartureTime <= now ||
                flight.DepartureTime > now.AddHours(24))
            {
                return RedirectToAction(nameof(OnlineBookings));
            }

            if (!registration.SeatId.HasValue)
            {
                return RedirectToAction(nameof(SelectSeat),
                    new { id = bookingPassengerId });
            }
            var seatExists = await _context.Seats
            .AnyAsync(s =>
                s.Id == registration.SeatId.Value &&
                s.AircraftId == flight.AircraftId);

            if (!seatExists)
                return BadRequest("Selected seat not belong to flight's aircraft");

            registration.Status = RegistrationStatus.Finished;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Online registration completed succesfully";

            return RedirectToAction(nameof(OnlineBookings));
        }
    }
}
