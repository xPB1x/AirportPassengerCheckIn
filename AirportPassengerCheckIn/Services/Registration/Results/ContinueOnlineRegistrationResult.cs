namespace AirportPassengerCheckIn.Services.Registration.Results
{
    namespace AirportPassengerCheckIn.Services.Results
    {
        public class ContinueOnlineRegistrationResult
        {
            public OnlineRegistrationError Error { get; set; }

            public OnlineRegistrationStep? NextStep { get; set; }

            public bool Succeeded => Error == OnlineRegistrationError.None;

            public static ContinueOnlineRegistrationResult Success(
                OnlineRegistrationStep nextStep)
            {
                return new ContinueOnlineRegistrationResult
                {
                    Error = OnlineRegistrationError.None,
                    NextStep = nextStep
                };
            }

            public static ContinueOnlineRegistrationResult Failure(
                OnlineRegistrationError error)
            {
                return new ContinueOnlineRegistrationResult
                {
                    Error = error,
                    NextStep = null
                };
            }
        }
    }
}
