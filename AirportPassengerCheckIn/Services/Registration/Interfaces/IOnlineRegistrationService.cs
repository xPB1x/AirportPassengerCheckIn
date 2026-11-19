using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Services.Registration.Results.AirportPassengerCheckIn.Services.Results;

namespace AirportPassengerCheckIn.Services.Registration.Interfaces
{
    public interface IOnlineRegistrationService
    {
        Task<OnlineRegistrationResult> CheckStartRegistrationAsync(
            string userId, int bookingPassengerId);

        Task<OnlineRegistrationResult> StartRegistrationAsync(
            string userId, int bookingPassengerId);

        Task<ContinueOnlineRegistrationResult> GetContinueStepOnlineAsync(
            string userId, int bookingPassengerId);

        Task<AvailableSeatsResult> GetAvailableSeatsAsync(
            string userId, int bookingPassengerId);

        Task<OnlineRegistrationResult> SelectSeatAsync(
            string userId, int bookingPassengerId, int selectedSeatId);

        Task<OnlineRegistrationResult> FinishRegistrationAsync(
            string userId, int bookingPassengerId);
    }
}
