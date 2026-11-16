namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class StartOnlineViewModel
    {
        public int BookingPassengerId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string PassengerFullName { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
    }
}
