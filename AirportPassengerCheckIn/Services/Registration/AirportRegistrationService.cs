using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Services.Registration.Results.AirportPassengerCheckIn.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Services.Registration
{
    public class AirportRegistrationService
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
            if (IsRegistrationWindowOpen(bookingPassenger.Booking.Flight.DepartureTime))
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

        //public async Task<ContinueOnlineRegistrationResult> GetContinueStepAsync(
        //    int bookingPassengerId)
        //{

        //}

        //public async Task<AvailableSeatsResult> GetAvailableSeatsAsync(
        //    int bookingPassengerId)
        //{

        //}

        //public async Task<RegistrationResult> SelectSeatAsync(
        //    int bookingPassengerId, int selectedSeatId)
        //{

        //}
        //public async Task<RegistrationResult> FinishRegistrationAsync(
        //    int bookingPassengerId)
        //{

        //}
        //public async Task<RegistrationResult> AddBaggage(
        //    int bookingPassengerId)
        //{

        //}
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
