using System.ComponentModel.DataAnnotations;

namespace AirportPassengerCheckIn.DTOs.ViewModels;

public class RegisterViewModel
{
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


    [Phone(ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Номер телефона")]
    public string? PhoneNumber { get; set; }
}