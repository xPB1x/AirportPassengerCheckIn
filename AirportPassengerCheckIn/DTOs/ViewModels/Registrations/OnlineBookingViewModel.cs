using AirportPassengerCheckIn.Models.Enums;

namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class OnlineBookingViewModel
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;

        public string FlightNumber { get; set; } = string.Empty;

        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }

        public List<OnlineBookingPassengerViewModel> Passengers { get; set; } = new();
    }
}