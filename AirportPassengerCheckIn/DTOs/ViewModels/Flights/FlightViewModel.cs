
using System.ComponentModel.DataAnnotations;

namespace AirportPassengerCheckIn.DTOs.ViewModels.Flights
{
    public class FlightViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Номер рейса")]
        public string FlightNumber { get; set; } = string.Empty;

        [Display(Name = "Время вылета")]
        public DateTime DepartureTime { get; set; }

        [Display(Name = "Время прибытия")]
        public DateTime ArrivalTime { get; set; }

        public string DepartureAirportCode { get; set; } = string.Empty;
        public string DepartureAirportCity { get; set; } = string.Empty;

        public string ArrivalAirportCode { get; set; } = string.Empty;
        public string ArrivalAirportCity { get; set; } = string.Empty;

        public string AircraftNumber { get; set; } = string.Empty;
        public string AircraftCompany { get; set; } = string.Empty;
        public string AircraftModel { get; set; } = string.Empty;
    }
}
