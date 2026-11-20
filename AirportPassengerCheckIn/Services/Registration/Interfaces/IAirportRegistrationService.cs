using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Services.Registration.Results.AirportPassengerCheckIn.Services.Results;

namespace AirportPassengerCheckIn.Services.Registration.Interfaces
{
    public interface IAirportRegistrationService
    {
        Task<RegistrationResult> StartRegistrationAsync(
            int bookingPassengerId);
        Task<ContinueOnlineRegistrationResult> GetContinueStepAsync(
            int bookingPassengerId);
        Task<AvailableSeatsResult> GetAvailableSeatsAsync(
            int bookingPassengerId);
        Task<RegistrationResult> SelectSeatAsync(
            int bookingPassengerId, int selectedSeatId);
        Task<RegistrationResult> FinishRegistrationAsync(
            int bookingPassengerId);
        Task<RegistrationResult> AddBaggage(
            int bookingPassengerId);
    }
}
