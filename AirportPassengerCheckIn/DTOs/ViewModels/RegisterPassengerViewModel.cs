using System.ComponentModel.DataAnnotations;

namespace AirportPassengerCheckIn.DTOs.ViewModels;

public class RegisterPassengerViewModel
{
    // User data

    [Required(ErrorMessage = "Укажите электронную почту")]
    [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль")]
    [StringLength(100, MinimumLength = 6,
        ErrorMessage = "Пароль должен содержать от 6 до 100 символов")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password),
        ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;


    // Passenger data

    [Required(ErrorMessage = "Укажите имя")]
    [StringLength(100)]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите фамилию")]
    [StringLength(100)]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Отчество")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Укажите дату рождения")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateOnly BirthDate { get; set; }

    [Required(ErrorMessage = "Укажите номер паспорта")]
    [StringLength(30)]
    [Display(Name = "Номер паспорта")]
    public string PassportNumber { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Номер телефона")]
    public string? PhoneNumber { get; set; }
}