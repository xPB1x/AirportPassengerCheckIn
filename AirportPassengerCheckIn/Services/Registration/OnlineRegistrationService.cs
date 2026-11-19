using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Services.Registration.Interfaces;
using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Services.Registration.Results.AirportPassengerCheckIn.Services.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Services.Registration
{
    public class OnlineRegistrationService : IOnlineRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeProvider _timeProvider;

        public OnlineRegistrationService(ApplicationDbContext context, TimeProvider timeProvider) {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<OnlineRegistrationResult> CheckStartRegistrationAsync(
            string userId,
            int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp =>
                    bp.Id == bookingPassengerId &&
                    bp.Booking.UserId == userId);

            if (bookingPassenger == null)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.BookingNotConfirmed);
            }

           
            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime)) 
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationWindowClosed);
            }

            if (bookingPassenger.Registration != null)
            {
                if (bookingPassenger.Registration.Type == RegistrationType.Online &&
                    bookingPassenger.Registration.Status == RegistrationStatus.Active)
                {
                    return OnlineRegistrationResult.Failure(
                        OnlineRegistrationError.RegistrationAlreadyActive);
                }

                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationAlreadyExists);
            }

            return OnlineRegistrationResult.Success();
        }

        public async Task<OnlineRegistrationResult> StartRegistrationAsync(
            string userId,
            int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp =>
                    bp.Id == bookingPassengerId &&
                    bp.Booking.UserId == userId);

            if (bookingPassenger == null)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.BookingNotConfirmed);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationWindowClosed);
            }

            if (bookingPassenger.Registration != null)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationAlreadyExists);
            }

            var registration = new Models.Registration
            {
                BookingPassengerId = bookingPassengerId,
                RegistrationTime = now,
                Type = RegistrationType.Online,
                Status = RegistrationStatus.Active
            };

            _context.Registrations.Add(registration);

            await _context.SaveChangesAsync();

            return OnlineRegistrationResult.Success();
        }

        public async Task<ContinueOnlineRegistrationResult> GetContinueStepOnlineAsync(
            string userId,
            int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp =>
                    bp.Id == bookingPassengerId &&
                    bp.Booking.UserId == userId);

            if (bookingPassenger == null)
            {
                return ContinueOnlineRegistrationResult.Failure(
                    OnlineRegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return ContinueOnlineRegistrationResult.Failure(
                    OnlineRegistrationError.BookingNotConfirmed);
            }

            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
            {
                return ContinueOnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationWindowClosed);
            }

            var registration = bookingPassenger.Registration;

            if (registration == null || registration.Status != RegistrationStatus.Active)
            {
                return ContinueOnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationNotActive);
            }

            if (registration.Type != RegistrationType.Online)
            {
                return ContinueOnlineRegistrationResult.Failure(
                    OnlineRegistrationError.InvalidRegistrationType);
            }


            var nextStep = registration.SeatId == null
                ? OnlineRegistrationStep.SelectSeat
                : OnlineRegistrationStep.Confirm;

            return ContinueOnlineRegistrationResult.Success(nextStep);
        }

        public async Task<AvailableSeatsResult> GetAvailableSeatsAsync(
            string userId,
            int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.Aircraft)
                            .ThenInclude(a => a.Seats)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp =>
                    bp.Id == bookingPassengerId &&
                    bp.Booking.UserId == userId);

            if (bookingPassenger == null)
            {
                return AvailableSeatsResult.Failure(
                    OnlineRegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return AvailableSeatsResult.Failure(
                    OnlineRegistrationError.BookingNotConfirmed);
            }


            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
                return AvailableSeatsResult.Failure(
                    OnlineRegistrationError.RegistrationWindowClosed);

            var registration = bookingPassenger.Registration;

            if (registration == null ||
                registration.Status != RegistrationStatus.Active)
            {
                return AvailableSeatsResult.Failure(
                    OnlineRegistrationError.RegistrationNotActive);
            }

            if (registration.Type != RegistrationType.Online)
            {
                return AvailableSeatsResult.Failure(
                    OnlineRegistrationError.InvalidRegistrationType);
            }

            var flightId = bookingPassenger.Booking.FlightId;

            var occupiedSeatIds = await _context.Registrations
                .Where(r =>
                    r.BookingPassenger.Booking.FlightId == flightId &&
                    r.BookingPassengerId != bookingPassengerId &&
                    r.SeatId != null &&
                    (r.Status == RegistrationStatus.Active ||
                     r.Status == RegistrationStatus.Finished))
                .Select(r => r.SeatId!.Value)
                .ToListAsync();

            var seats = bookingPassenger.Booking.Flight.Aircraft.Seats
                .Select(seat => new AvailableSeat
                {
                    Id = seat.Id,
                    SeatNumber = seat.SeatNumber,
                    Class = seat.Class,
                    IsOccupied = occupiedSeatIds.Contains(seat.Id)
                })
                .ToList();

            return AvailableSeatsResult.Success(seats);
        }

        public async Task<OnlineRegistrationResult> SelectSeatAsync(
            string userId, int bookingPassengerId, int selectedSeatId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Registration)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId && bp.Booking.UserId == userId);

            if (bookingPassenger == null)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.NotFound);
            }

            var booking = bookingPassenger.Booking;

            if (booking.Status != BookingStatus.Confirmed)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.BookingNotConfirmed);
            }

            var flight = booking.Flight;

            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationWindowClosed);
            }

            var registration = bookingPassenger.Registration;
            if (registration == null || registration.Status != RegistrationStatus.Active)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationNotActive);
            }

            if (registration.Type != RegistrationType.Online)
            {
                return OnlineRegistrationResult.Failure(OnlineRegistrationError.InvalidRegistrationType);
            }

            var seat = await _context.Seats
                .FirstOrDefaultAsync(s => s.Id == selectedSeatId);

            if (seat == null)
            {
                return OnlineRegistrationResult.Failure(OnlineRegistrationError.SeatNotFound);
            }

            if (seat.AircraftId != flight.AircraftId)
            {
                return OnlineRegistrationResult.Failure(OnlineRegistrationError.SeatBelongsToAnotherAircraft);
            }

            var isOccupied = await _context.Registrations.AnyAsync(r =>
                r.SeatId == selectedSeatId &&
                r.BookingPassenger.Booking.FlightId == flight.Id &&
                r.BookingPassengerId != bookingPassengerId &&
                (r.Status == RegistrationStatus.Active ||
                 r.Status == RegistrationStatus.Finished));

            if (isOccupied)
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.SeatOccupied);

            registration.SeatId = selectedSeatId;
            await _context.SaveChangesAsync();
            return OnlineRegistrationResult.Success();
        }

        public async Task<OnlineRegistrationResult> FinishRegistrationAsync(
            string userId,
            int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                    .ThenInclude(r => r!.Seat)
                .FirstOrDefaultAsync(bp =>
                    bp.Id == bookingPassengerId &&
                    bp.Booking.UserId == userId);

            if (bookingPassenger == null)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.BookingNotConfirmed);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var flight = bookingPassenger.Booking.Flight;

            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationWindowClosed);
            }

            var registration = bookingPassenger.Registration;

            if (registration == null ||
                registration.Status != RegistrationStatus.Active)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.RegistrationNotActive);
            }

            if (registration.Type != RegistrationType.Online)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.InvalidRegistrationType);
            }

            if (registration.SeatId == null)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.SeatNotSelected);
            }

            if (registration.Seat == null ||
                registration.Seat.AircraftId != flight.AircraftId)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.SeatBelongsToAnotherAircraft);
            }

            var isOccupied = await _context.Registrations.AnyAsync(r =>
                r.SeatId == registration.SeatId &&
                r.BookingPassenger.Booking.FlightId == flight.Id &&
                r.BookingPassengerId != bookingPassengerId &&
                (r.Status == RegistrationStatus.Active ||
                 r.Status == RegistrationStatus.Finished));

            if (isOccupied)
            {
                return OnlineRegistrationResult.Failure(
                    OnlineRegistrationError.SeatOccupied);
            }

            registration.Status = RegistrationStatus.Finished;

            await _context.SaveChangesAsync();

            return OnlineRegistrationResult.Success();
        }

        private bool IsRegistrationWindowOpen(DateTime departureTime)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (departureTime <= now || departureTime > now.AddHours(24))
            {
                return false;
            }
            return true;
        }
    }
}
