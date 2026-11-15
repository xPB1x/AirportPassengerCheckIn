
using System.ComponentModel.DataAnnotations;

namespace AirportPassengerCheckIn.DTOs.ViewModels
{
    public class CreateEmployeeViewModel
    {
        [Required(ErrorMessage = "Укажите имя")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите фамилию")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Укажите должность")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторите пароль")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
