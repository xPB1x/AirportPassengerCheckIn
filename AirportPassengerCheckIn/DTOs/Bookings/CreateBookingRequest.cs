using AirportPassengerCheckIn.Models;

namespace AirportPassengerCheckIn.DTOs.Bookings
{
    public class CreateBookingRequest
    {
        public int FlightId;
        public List<CreateBookingPassengerRequest> Passengers { get; set; } = new();
    }
}
