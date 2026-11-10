namespace AirportPassengerCheckIn.Models
{
    public class Airport
    {
        public int Id { get; set; }
        public String Name { get; set; } = String.Empty;
        public String CodeIATA { get; set; } = String.Empty;
        public String Country { get; set; } = String.Empty;
        public String City { get; set; } = String.Empty;
    }
}
