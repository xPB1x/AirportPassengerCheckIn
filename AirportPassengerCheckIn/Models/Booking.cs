namespace AirportPassengerCheckIn.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public String BookingNumber { get; set; } = String.Empty;
        public DateTime CreatedAt { get; set; }
        public int FlightId { get; set; }
        public Flight Flight { get; set; } = null!;
        public ICollection<BookingPassenger> BookingPassengers { get; set; } = new List<BookingPassenger>();
    }
}
