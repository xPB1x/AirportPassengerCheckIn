namespace AirportPassengerCheckIn.DTOs
{
    public class BookingDto
    {
        public string BookingNumber { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public FlightDto Flight { get; set; }

        public List<BookingPassengerDto> Passengers { get; set; }
    }


}
