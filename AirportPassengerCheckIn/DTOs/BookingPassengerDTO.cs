namespace AirportPassengerCheckIn.DTOs
{
    public class BookingPassengerDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PassportNumber { get; set; }

        public string RegistrationStatus { get; set; }
        public string SeatNumber { get; set; }
    }
}
