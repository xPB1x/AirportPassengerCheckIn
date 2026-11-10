namespace AirportPassengerCheckIn.Models
{
    public class Baggage
    {
        public int Id { get; set; }
        public float Weight { get; set; }
        public Boolean IsOversized { get; set; }
        public int RegistrationId { get; set; }
        public Registration Registration { get; set; } = null!;
    }
}
