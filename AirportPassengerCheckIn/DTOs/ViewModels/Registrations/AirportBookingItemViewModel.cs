namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class AirportBookingItemViewModel
    {
        public int BookingPassengerId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string? DepartureTimeZoneId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string? RegistrationStatus { get; set; }
        public string? RegistrationType { get; set; }
        public string? SeatNumber { get; set; }
        public int CountPaidBaggage { get; set; }
        public int RegisteredBaggageCount { get; set; }
    }
}
