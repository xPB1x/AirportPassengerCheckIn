namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class SelectSeatViewModel
    {
        public int BookingPassengerId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public int? SelectedSeatId { get; set; }
        public List<SeatViewModel> Seats { get; set; } = new();
    }
}
