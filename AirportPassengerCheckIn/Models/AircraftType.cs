namespace AirportPassengerCheckIn.Models
{
    public class AircraftType
    {
        public int Id { get; set; }
        public String Company { get; set; } = String.Empty;
        public String Model { get; set; } = String.Empty;
        public int NumberOfSeats { get; set; }
        public ICollection<Aircraft> Aircrafts { get; set; } = new List<Aircraft>();
        
    }
}
