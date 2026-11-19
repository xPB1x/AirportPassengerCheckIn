using AirportPassengerCheckIn.Models.Enums;

namespace AirportPassengerCheckIn.Models
{
    public class BookingPassenger
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int PassengerId { get; set; }
        public Passenger Passenger { get; set; } = null!;
        public Registration? Registration { get; set; }
        public int CountPaidBaggages { get; set; } = 0;
        public SeatClass SeatClass { get; set; }
    }
}
