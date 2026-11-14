using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Models.Identity;

namespace AirportPassengerCheckIn.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; } = String.Empty;
        public ApplicationUser User { get; set; } = null!;
        public String BookingNumber { get; set; } = String.Empty;
        public DateTime CreatedAt { get; set; }
        public BookingStatus Status { get; set; }
        public int FlightId { get; set; }
        public Flight Flight { get; set; } = null!;
        public ICollection<BookingPassenger> BookingPassengers { get; set; } = new List<BookingPassenger>();
    }
}
