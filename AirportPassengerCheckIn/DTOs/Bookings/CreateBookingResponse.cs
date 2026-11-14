namespace AirportPassengerCheckIn.DTOs.Bookings
{
    public class CreateBookingResponse
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int FlightId { get; set; }
        public int PassengerCount { get; set; }
    }
}
