namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class ConfirmOnlineViewModel
    {
        public int BookingPassengerId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
    }
}