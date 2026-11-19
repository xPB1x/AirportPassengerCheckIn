namespace AirportPassengerCheckIn.Services.Registration.Results
{
    public enum OnlineRegistrationError
    {
        None,
        NotFound,
        BookingNotConfirmed,
        RegistrationWindowClosed,

        RegistrationAlreadyExists,
        RegistrationAlreadyActive,
        RegistrationNotActive,
        InvalidRegistrationType,

        SeatNotFound,
        SeatBelongsToAnotherAircraft,
        SeatOccupied,
        SeatNotSelected,
    }
}
