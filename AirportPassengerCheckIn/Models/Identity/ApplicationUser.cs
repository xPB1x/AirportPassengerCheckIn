using Microsoft.AspNetCore.Identity;

namespace AirportPassengerCheckIn.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int? PassengerId { get; set; }
        public Passenger? Passenger { get; set; }
        public int? AirportEmployeeId { get; set; }
        public AirportEmployee? AirportEmployee { get; set; }
    }
}
