using AirportPassengerCheckIn.Models.Enums;

namespace AirportPassengerCheckIn.Services.Registration.Results
{
    public class AvailableSeatsResult
    {
        public OnlineRegistrationError Error { get; set; }

        public List<AvailableSeat> Seats { get; set; } = new();

        public bool Succeeded => Error == OnlineRegistrationError.None;

        public static AvailableSeatsResult Success(
            List<AvailableSeat> seats)
        {
            return new AvailableSeatsResult
            {
                Error = OnlineRegistrationError.None,
                Seats = seats
            };
        }

        public static AvailableSeatsResult Failure(
            OnlineRegistrationError error)
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
