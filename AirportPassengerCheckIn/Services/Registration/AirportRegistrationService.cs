using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs.ViewModels.Registrations;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Services.Registration.Interfaces;
using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Services.Registration.Results.AirportPassengerCheckIn.Services.Results;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirportPassengerCheckIn.Services.Registration
{
    public class AirportRegistrationService : IAirportRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeProvider _timeProvider;

        public AirportRegistrationService(
            ApplicationDbContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<RegistrationResult> StartRegistrationAsync(
            string userId, int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                 .Include(bp => bp.Booking)
                     .ThenInclude(b => b.Flight)
                 .Include(bp => bp.Registration)
                 .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return RegistrationResult.Failure(
                    RegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return RegistrationResult.Failure(
                    RegistrationError.BookingNotConfirmed);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
            {
                return RegistrationResult.Failure(
                    RegistrationError.RegistrationWindowClosed);
            }

            if (bookingPassenger.Registration != null)
            {
                return RegistrationResult.Failure(
                    RegistrationError.RegistrationAlreadyExists);
            }

            var airportEmployeeId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.AirportEmployeeId)
                .FirstOrDefaultAsync();

            var registration = new Models.Registration
            {
                RegistrationTime = now,
                Type = RegistrationType.Airport,
                Status = RegistrationStatus.Active,
                BookingPassengerId = bookingPassenger.Id,
                AirportEmployeeId = airportEmployeeId
            };

            _context.Registrations.Add(registration);

            await _context.SaveChangesAsync();

            return RegistrationResult.Success();
        }

        public async Task<ContinueOnlineRegistrationResult> GetContinueStepAsync(
            string userId, int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return ContinueOnlineRegistrationResult.Failure(
                    RegistrationError.NotFound);
            }

            var booking = bookingPassenger!.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return ContinueOnlineRegistrationResult.Failure(
                    RegistrationError.BookingNotConfirmed);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
            {
                return ContinueOnlineRegistrationResult.Failure(
                    RegistrationError.RegistrationWindowClosed);
            }

            if (registration == null || registration.Status != RegistrationStatus.Active)
            {
                return ContinueOnlineRegistrationResult.Failure(
                    RegistrationError.RegistrationNotActive);
            }
            
            if (registration.Type == RegistrationType.Online)
            {
                var airportEmployeeId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.AirportEmployeeId)
                    .FirstOrDefaultAsync();

                _context.Registrations.Remove(registration);

                registration = new Models.Registration
                {
                    RegistrationTime = now,
                    Type = RegistrationType.Airport,
                    Status = RegistrationStatus.Active,
                    BookingPassengerId = bookingPassenger.Id,
                    AirportEmployeeId = airportEmployeeId
                };

                _context.Registrations.Add(registration);

                await _context.SaveChangesAsync();
            }

            else if (registration.Type != RegistrationType.Airport)
            {
                return ContinueOnlineRegistrationResult.Failure(
                    RegistrationError.InvalidRegistrationType);
            }

            if (registration.SeatId == null)
            {
                return ContinueOnlineRegistrationResult.Success(
                    RegistrationStep.SelectSeat);
            }

            return ContinueOnlineRegistrationResult.Success(
                    RegistrationStep.Confirm);
        }

        public async Task<AvailableSeatsResult> GetAvailableSeatsAsync(
            int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                        .ThenInclude(f => f.Aircraft)
                            .ThenInclude(a => a.Seats)
                .Include(bp => bp.Registration)
                .FirstOrDefaultAsync(bp =>
                    bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return AvailableSeatsResult.Failure(
                    RegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return AvailableSeatsResult.Failure(
                    RegistrationError.BookingNotConfirmed);
            }


            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
                return AvailableSeatsResult.Failure(
                    RegistrationError.RegistrationWindowClosed);

            var registration = bookingPassenger.Registration;

            if (registration == null ||
                registration.Status != RegistrationStatus.Active)
            {
                return AvailableSeatsResult.Failure(
                    RegistrationError.RegistrationNotActive);
            }

            if (registration.Type != RegistrationType.Online)
            {
                return AvailableSeatsResult.Failure(
                    RegistrationError.InvalidRegistrationType);
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

        public async Task<RegistrationResult> SelectSeatAsync(
            int bookingPassengerId, int selectedSeatId)
        {
            var bookingPassenger = await _context.BookingPassengers
               .Include(bp => bp.Registration)
               .Include(bp => bp.Booking)
                   .ThenInclude(b => b.Flight)
               .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return RegistrationResult.Failure(
                    RegistrationError.NotFound);
            }

            var booking = bookingPassenger.Booking;

            if (booking.Status != BookingStatus.Confirmed)
            {
                return RegistrationResult.Failure(
                    RegistrationError.BookingNotConfirmed);
            }

            var flight = booking.Flight;

            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
            {
                return RegistrationResult.Failure(
                    RegistrationError.RegistrationWindowClosed);
            }

            var registration = bookingPassenger.Registration;
            if (registration == null || registration.Status != RegistrationStatus.Active)
            {
                return RegistrationResult.Failure(
                    RegistrationError.RegistrationNotActive);
            }

            if (registration.Type != RegistrationType.Online)
            {
                return RegistrationResult.Failure(RegistrationError.InvalidRegistrationType);
            }

            var seat = await _context.Seats.FirstOrDefaultAsync(
                s => s.Id == selectedSeatId);

            if (seat == null)
            {
                return RegistrationResult.Failure(
                    RegistrationError.SeatNotFound);
            }
            if (seat.AircraftId != flight.AircraftId)
            {
                return RegistrationResult.Failure(
                    RegistrationError.SeatBelongsToAnotherAircraft);
            }

            if (seat.Class != bookingPassenger.SeatClass)
            {
                return RegistrationResult.Failure(
                    RegistrationError.DifferentSeatClasses);
            }

            var isOccupied = await _context.Registrations
                .AnyAsync(r =>
                    r.BookingPassenger.Booking.Flight.Id == flight.Id &&
                    r.BookingPassengerId != bookingPassengerId &&
                    r.SeatId == seat.Id &&
                    (r.Status == RegistrationStatus.Active || 
                    r.Status == RegistrationStatus.Finished));

            if (isOccupied)
            {
                return RegistrationResult.Failure(
                    RegistrationError.SeatOccupied);
            }

            registration.SeatId = seat.Id;
            await _context.SaveChangesAsync();

            return RegistrationResult.Success();
        }

        public async Task<RegistrationResult> FinishRegistrationAsync(
            int bookingPassengerId)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                    .ThenInclude(r => r!.Seat)
                .Include(bp => bp.Passenger)
                .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return RegistrationResult.Failure(
                    RegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return RegistrationResult.Failure(
                    RegistrationError.BookingNotConfirmed);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var flight = bookingPassenger.Booking.Flight;

            if (!IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
            {
                return RegistrationResult.Failure(
                    RegistrationError.RegistrationWindowClosed);
            }

            var registration = bookingPassenger.Registration;

            if (registration == null ||
                registration.Status != RegistrationStatus.Active)
            {
                return RegistrationResult.Failure(
                    RegistrationError.RegistrationNotActive);
            }

            if (registration.Type != RegistrationType.Online)
            {
                return RegistrationResult.Failure(
                    RegistrationError.InvalidRegistrationType);
            }

            if (registration.SeatId == null)
            {
                return RegistrationResult.Failure(
                    RegistrationError.SeatNotSelected);
            }

            if (registration.Seat == null ||
                registration.Seat.AircraftId != flight.AircraftId)
            {
                return RegistrationResult.Failure(
                    RegistrationError.SeatBelongsToAnotherAircraft);
            }

            var isOccupied = await _context.Registrations.AnyAsync(r =>
                r.SeatId == registration.SeatId &&
                r.BookingPassenger.Booking.FlightId == flight.Id &&
                r.BookingPassengerId != bookingPassengerId &&
                (r.Status == RegistrationStatus.Active ||
                 r.Status == RegistrationStatus.Finished));

            if (isOccupied)
            {
                return RegistrationResult.Failure(
                    RegistrationError.SeatOccupied);
            }

            registration.Status = RegistrationStatus.Finished;

            await _context.SaveChangesAsync();

            return RegistrationResult.Success();
        }


        public async Task<RegistrationResult> AddBaggage(
            int bookingPassengerId, BaggageViewModel model)
        {
            var bookingPassenger = await _context.BookingPassengers
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(bp => bp.Registration)
                    .ThenInclude(r => r!.Baggages)
                .FirstOrDefaultAsync(bp => bp.Id == bookingPassengerId);

            if (bookingPassenger == null)
            {
                return RegistrationResult.Failure(
                    RegistrationError.NotFound);
            }

            if (bookingPassenger.Booking.Status != BookingStatus.Confirmed)
            {
                return RegistrationResult.Failure(
                    RegistrationError.BookingNotConfirmed);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var booking = bookingPassenger.Booking;
            var flight = booking.Flight;
            var registration = bookingPassenger.Registration;

            if (!IsRegistrationWindowOpen(flight.DepartureTime))
            {
                return RegistrationResult.Failure(
                    RegistrationError.RegistrationWindowClosed);
            }

            if (registration == null)
            {
                return RegistrationResult.Failure(
                    RegistrationError.RegistrationNotExists);
            }
            if (registration.Status != RegistrationStatus.Finished)
            {
                return RegistrationResult.Failure(
                    RegistrationError.InvalidRegistrationType);
            }

            if (bookingPassenger.CountPaidBaggages <= registration.Baggages.Count)
            {
                return RegistrationResult.Failure(
                    RegistrationError.InvalidPaidBaggagesCount);
            }

            if (model.Weight <= 0 ||
                model.Weight > flight.MaxBaggageWeight)
            {
                return RegistrationResult.Failure(
                    RegistrationError.InvalifBaggageWeight);
            }

            var baggage = new Baggage
            {
                Weight = model.Weight,
                IsOversized = model.IsOversized,
                RegistrationId = registration.Id
            };

            _context.Baggages.Add(baggage);

            await _context.SaveChangesAsync();
            return RegistrationResult.Success();
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
