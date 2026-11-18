namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class BaggageViewModel
    {
        public int BookingPassengerId { get; set; }

        public string PassengerName { get; set; } = string.Empty;
        public string BookingNumber { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;

        public int CountPaidBaggage { get; set; }
        public decimal MaxBaggageWeight { get; set; }

        public decimal Weight { get; set; }
        public bool IsOversized { get; set; }

        public List<BaggageItemViewModel> Baggages { get; set; } = new();

        public int RemainingBaggageCount =>
            Math.Max(0, CountPaidBaggage - Baggages.Count);
    }   
}
