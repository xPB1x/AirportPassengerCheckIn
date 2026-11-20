using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Services.Registration.Results.AirportPassengerCheckIn.Services.Results;

namespace AirportPassengerCheckIn.Services.Registration.Interfaces
{
    public interface IOnlineRegistrationService
    {
        Task<RegistrationResult> CheckStartRegistrationAsync(
            string userId, int bookingPassengerId);

        Task<RegistrationResult> StartRegistrationAsync(
            string userId, int bookingPassengerId);

        Task<ContinueOnlineRegistrationResult> GetContinueStepOnlineAsync(
            string userId, int bookingPassengerId);

        Task<AvailableSeatsResult> GetAvailableSeatsAsync(
            string userId, int bookingPassengerId);

        Task<RegistrationResult> SelectSeatAsync(
            string userId, int bookingPassengerId, int selectedSeatId);

        Task<RegistrationResult> FinishRegistrationAsync(
            string userId, int bookingPassengerId);
    }
}
