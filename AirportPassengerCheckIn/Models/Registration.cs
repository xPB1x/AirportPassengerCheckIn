using AirportPassengerCheckIn.Models.Enums;

namespace AirportPassengerCheckIn.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public DateTime RegistrationTime { get; set; }
        public RegistrationType Type { get; set; }
        public RegistrationStatus Status { get; set; }
        public DateTime? CancellationDateTime { get; set; }
        public int BookingPassengerId { get; set; }
        public BookingPassenger BookingPassenger { get; set; } = null!;
        public int? SeatId { get; set; }
        public Seat? Seat { get; set; }
        public int? AirportEmployeeId { get; set; }
        public AirportEmployee? AirportEmployee { get; set; }
        public ICollection<Baggage> Baggages { get; set; } = new List<Baggage>();
    }
}
