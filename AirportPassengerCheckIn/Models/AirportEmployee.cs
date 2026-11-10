namespace AirportPassengerCheckIn.Models
{
    public class AirportEmployee
    {
        public int Id { get; set; }
        public String FirstName { get; set; } = String.Empty;
        public String LastName { get; set; } = String.Empty;
        public String Position { get; set; } = String.Empty;
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
