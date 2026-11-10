namespace AirportPassengerCheckIn.Models
{
    public class Aircraft
    {
        public int Id { get; set; }
        public String AircraftNumber { get; set; } = String.Empty;
        public int AircraftTypeId { get; set; }
        public AircraftType AircraftType { get; set; } = null!;
        public ICollection<Flight> Flights { get; set; } = new List<Flight>();
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
