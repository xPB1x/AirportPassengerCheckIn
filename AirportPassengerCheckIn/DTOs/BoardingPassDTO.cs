namespace AirportPassengerCheckIn.DTOs
{
    public class BoardingPassDTO
    {
        public String PassengerName { get; set; } = String.Empty;
        public String BookingNumber { get; set; } = String.Empty;
        public String FlightNumber { get; set; } = String.Empty;
        public String SeatNumber { get; set; } = String.Empty;
        public String DepartureAirport { get; set; } = String.Empty;
        public String ArrivalAirport { get; set; } = String.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime RegistrationTime { get; set; }

    }
}
