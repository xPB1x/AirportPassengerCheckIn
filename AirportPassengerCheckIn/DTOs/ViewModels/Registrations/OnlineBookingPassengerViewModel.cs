using AirportPassengerCheckIn.Models.Enums;

namespace AirportPassengerCheckIn.DTOs.ViewModels
{
    public class OnlineBookingPassengerViewModel
    {
        public int BookingPassengerId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public RegistrationStatus? RegistrationStatus { get; set; }

        public bool CanRegister { get; set; }
        public bool HasActiveRegistration { get; set; }
        public bool IsRegistrationFinished { get; set; }
    }
}