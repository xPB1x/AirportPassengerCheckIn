using System.ComponentModel.DataAnnotations;

namespace AirportPassengerCheckIn.DTOs.Bookings
{
    public class CreateBookingPassengerRequest
    {
        [Required(ErrorMessage = "Укажите имя")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите фамилию")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]

        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите дату рождения")]
        [DataType(DataType.Date)]
        public DateOnly? BirthDate { get; set; }

        [Required(ErrorMessage = "Укажите номер пасспорта")]
        [StringLength(30)]
        public string PassportNumber { get; set; } = string.Empty;

    }
}
