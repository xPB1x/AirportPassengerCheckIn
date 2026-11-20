using AirportPassengerCheckIn.Models.Enums;

namespace AirportPassengerCheckIn.Services.Registration.Results
{
    public class AvailableSeatsResult
    {
        public RegistrationError Error { get; set; }

        public List<AvailableSeat> Seats { get; set; } = new();

        public bool Succeeded => Error == RegistrationError.None;

        public static AvailableSeatsResult Success(
            List<AvailableSeat> seats)
        {
            return new AvailableSeatsResult
            {
                Error = RegistrationError.None,
                Seats = seats
            };
        }

        public static AvailableSeatsResult Failure(
            RegistrationError error)
        {
            return new AvailableSeatsResult
            {
                Error = error
            };
        }
    }

    public class AvailableSeat
    {
        public int Id { get; set; }

        public string SeatNumber { get; set; } = string.Empty;

        public SeatClass Class { get; set; }

        public bool IsOccupied { get; set; }
    }
}
