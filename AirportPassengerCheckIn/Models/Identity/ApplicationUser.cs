using Microsoft.AspNetCore.Identity;

namespace AirportPassengerCheckIn.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public int? AirportEmployeeId { get; set; }
        public AirportEmployee? AirportEmployee { get; set; }
    }
}
