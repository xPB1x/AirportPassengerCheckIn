namespace AirportPassengerCheckIn.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public String SeatNumber { get; set; } = String.Empty;
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; } = null!;
    }
}
