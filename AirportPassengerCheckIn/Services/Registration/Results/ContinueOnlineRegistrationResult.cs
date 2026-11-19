namespace AirportPassengerCheckIn.Services.Registration.Results
{
    namespace AirportPassengerCheckIn.Services.Results
    {
        public class OnlineRegistrationContinuationResult
        {
            public OnlineRegistrationError Error { get; set; }

            public OnlineRegistrationStep? NextStep { get; set; }

            public bool Succeeded => Error == OnlineRegistrationError.None;

            public static OnlineRegistrationContinuationResult Success(
                OnlineRegistrationStep nextStep)
            {
                return new OnlineRegistrationContinuationResult
                {
                    Error = OnlineRegistrationError.None,
                    NextStep = nextStep
                };
            }

            public static OnlineRegistrationContinuationResult Failure(
                OnlineRegistrationError error)
            {
                return new OnlineRegistrationContinuationResult
                {
                    Error = error,
                    NextStep = null
                };
            }
        }
    }
}
