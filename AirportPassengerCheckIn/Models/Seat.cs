namespace AirportPassengerCheckIn.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public String seatNumber { get; set; } = String.Empty;
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; } = null!;
    }
}
