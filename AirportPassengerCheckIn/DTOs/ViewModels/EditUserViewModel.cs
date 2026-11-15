using System.ComponentModel.DataAnnotations;

namespace AirportPassengerCheckIn.DTOs.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public string userId { get; set; } = string.Empty;
    }
}
