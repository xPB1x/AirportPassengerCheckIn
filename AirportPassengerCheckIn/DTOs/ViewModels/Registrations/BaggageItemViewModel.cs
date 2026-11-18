namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class BaggageItemViewModel
    {
        public int Id { get; set; }
        public decimal Weight { get; set; }
        public bool IsOversized { get; set; }
    }
}
