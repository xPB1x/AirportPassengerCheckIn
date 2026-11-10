namespace AirportPassengerCheckIn.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public String FlightNumber { get; set; } = String.Empty;
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public int DepartureAirportId { get; set; }
        public Airport DepartureAirport { get; set; } = null!;
        public int ArrivalAirportId { get; set; }
        public Airport ArrivalAirport { get; set; } = null!;
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
    }
}
