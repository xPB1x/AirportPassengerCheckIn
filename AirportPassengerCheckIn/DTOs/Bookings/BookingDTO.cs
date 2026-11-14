namespace AirportPassengerCheckIn.DTOs.Bookings
{
    public class BookingDTO
    {
        public string BookingNumber { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public FlightDTO Flight { get; set; }

        public List<BookingPassengerDTO> Passengers { get; set; }
    }


}
