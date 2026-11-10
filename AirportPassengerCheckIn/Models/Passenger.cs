namespace AirportPassengerCheckIn.Models
{
    public class Passenger
    {
        public int Id { get; set; }
        public String FirstName { get; set; } = String.Empty;
        public String LastName { get; set; } = String.Empty;
        public String? MiddleName { get; set; }
        public DateOnly BirthDate { get; set; }
        public String PassportNumber { get; set; } = String.Empty;
        public ICollection<BookingPassenger> BookingPassengers { get; set; } = new List<BookingPassenger>();
    }
}
