using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AirportPassengerCheckIn.DTOs.ViewModels.Flights
{
    public class EditFlightViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите номер рейса")]
        [StringLength(20, ErrorMessage = "Номер рейса не должен превышать 20 символов")]
        [Display(Name = "Номер рейса")]
        public string FlightNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите время вылета")]
        [Display(Name = "Время вылета")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Укажите время прибытия")]
        [Display(Name = "Время прибытия")]
        public DateTime ArrivalTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите аэропорт вылета")]
        [Display(Name = "Аэропорт вылета")]
        public int DepartureAirportId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите аэропорт прибытия")]
        [Display(Name = "Аэропорт прибытия")]
        public int ArrivalAirportId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите самолёт")]
        [Display(Name = "Самолёт")]
        public int AircraftId { get; set; }

        public List<SelectListItem> Airports { get; set; } = new();
        public List<SelectListItem> Aircrafts { get; set; } = new();
    }
}