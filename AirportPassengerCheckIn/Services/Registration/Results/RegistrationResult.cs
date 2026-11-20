namespace AirportPassengerCheckIn.Services.Registration.Results
{
    public class RegistrationResult
    {
        public RegistrationError Error { get; set; }
        public bool Succeeded => Error == RegistrationError.None;

        public static RegistrationResult Success()
        {
            return new RegistrationResult
            {
                Error = RegistrationError.None
            };
        }

        public static RegistrationResult Failure(RegistrationError error)
        {
            return new RegistrationResult
            {
                Error = error
            };
        }
    }
}
