namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class AirportBookingViewModel
    {
        public string PassportNumber { get; set; } = string.Empty;
        public List<AirportBookingItemViewModel> Bookings { get; set; } = new();
    }
}
