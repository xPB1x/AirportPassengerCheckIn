namespace AirportPassengerCheckIn.Services.Registration.Results
{
    namespace AirportPassengerCheckIn.Services.Results
    {
        public class ContinueOnlineRegistrationResult
        {
            public RegistrationError Error { get; set; }

            public RegistrationStep? NextStep { get; set; }

            public bool Succeeded => Error == RegistrationError.None;

            public static ContinueOnlineRegistrationResult Success(
                RegistrationStep nextStep)
            {
                return new ContinueOnlineRegistrationResult
                {
                    Error = RegistrationError.None,
                    NextStep = nextStep
                };
            }

            public static ContinueOnlineRegistrationResult Failure(
                RegistrationError error)
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
