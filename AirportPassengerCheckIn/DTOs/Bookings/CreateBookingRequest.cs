using AirportPassengerCheckIn.Models;

namespace AirportPassengerCheckIn.DTOs.Bookings
{
    public class CreateBookingRequest
    {
        public int FlightId { get; set; }
        public List<CreateBookingPassengerRequest> Passengers { get; set; } = new();
    }
}
