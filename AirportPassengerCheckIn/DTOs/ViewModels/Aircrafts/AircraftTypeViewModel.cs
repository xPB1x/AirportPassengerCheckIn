
using System.ComponentModel.DataAnnotations;

namespace AirportPassengerCheckIn.DTOs.ViewModels.Aircrafts;

public class AircraftTypeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите производителя")]
    [StringLength(100, ErrorMessage = "Название производителя не должно превышать 100 символов")]
    [Display(Name = "Производитель")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите модель самолёта")]
    [StringLength(100, ErrorMessage = "Название модели не должно превышать 100 символов")]
    [Display(Name = "Модель")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите количество мест")]
    [Range(1, 1000, ErrorMessage = "Количество мест должно быть от 1 до 1000")]
    [Display(Name = "Количество мест")]
    public int NumberOfSeats { get; set; }
    public int AircraftCount { get; set; }
}
