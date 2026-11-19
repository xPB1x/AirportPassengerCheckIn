using AirportPassengerCheckIn.Services.Registration.Results;

namespace AirportPassengerCheckIn.Services.Registration.Interfaces
{
    public interface IOnlineRegistrationService
    {
        Task<OnlineRegistrationResult> CheckStartRegistrationAsync(
            string userId, int bookingPassengerId);

        Task<OnlineRegistrationResult> StartRegistrationAsync(
            string userId, int bookingPassengerId);

        Task<OnlineRegistrationResult> GetContinueStepOnline(
            string userId, int bookingPassengerId);

        Task<OnlineRegistrationResult> GetAvailableSeatsAsync(
            string userId, int bookingPassengerId);

        Task<OnlineRegistrationResult> SelectSeatAsync(
            string userId, int bookingPassengerId, int selectedSeatId);

        Task<OnlineRegistrationResult> FinishRegistrationAsync(
            string userId, int bookingPassengerId);
    }
}
