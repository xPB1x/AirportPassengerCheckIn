namespace AirportPassengerCheckIn.Services.Registration.Results
{
    public enum RegistrationError
    {
        None,
        NotFound,
        BookingNotConfirmed,
        RegistrationWindowClosed,

        RegistrationAlreadyExists,
        RegistrationNotExists,
        RegistrationAlreadyActive,
        RegistrationNotActive,
        InvalidRegistrationType,

        SeatNotFound,
        SeatBelongsToAnotherAircraft,
        SeatOccupied,
        SeatNotSelected,
        DifferentSeatClasses,

        InvalidPaidBaggagesCount,
        InvalifBaggageWeight,
    }
}
