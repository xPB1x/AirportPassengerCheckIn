namespace AirportPassengerCheckIn.Services.Registration.Results
{
    public class OnlineRegistrationResult
    {
        public OnlineRegistrationError Error { get; set; }
        public bool Succeeded => Error == OnlineRegistrationError.None;

        public static OnlineRegistrationResult Success()
        {
            return new OnlineRegistrationResult
            {
                Error = OnlineRegistrationError.None
            };
        }

        public static OnlineRegistrationResult Failure(OnlineRegistrationError error)
        {
            return new OnlineRegistrationResult
            {
                Error = error
            };
        }
    }
}
