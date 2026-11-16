using AirportPassengerCheckIn.Models.Enums;

namespace AirportPassengerCheckIn.DTOs.ViewModels.Registrations
{
    public class SeatViewModel
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public SeatClass Class { get; set; }
        public bool IsOccupied { get; set; }
    }
}
