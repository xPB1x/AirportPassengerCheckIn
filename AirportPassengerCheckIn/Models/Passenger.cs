namespace AirportPassengerCheckIn.Models
{
    public class Passenger
    {
        private int Id { get; set; }
        public String FirstName { get; set; } = String.Empty;
        public String SecondName { get; set; } = String.Empty;
        public String? MiddleName { get; set; }
        public DateOnly BirthDate { get; set; }
        public String PassportNumber { get; set; } = String.Empty;
        public ICollection<BookingPassenger> BookingPassengers { get; set; } = new List<BookingPassenger>();
    }
}
