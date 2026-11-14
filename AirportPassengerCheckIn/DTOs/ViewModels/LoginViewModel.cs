using System.ComponentModel.DataAnnotations;

namespace AirportPassengerCheckIn.DTOs.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Укажите электронную почту")]
        [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Укажите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;
        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
