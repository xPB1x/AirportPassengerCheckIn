using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs.ViewModels.Flights;
using AirportPassengerCheckIn.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FlightsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FlightsController (ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var flights = await _context.Flights
                .Select(f => new FlightViewModel
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,

                    DepartureAirportCode = f.DepartureAirport.CodeIATA,
                    DepartureAirportCity = f.DepartureAirport.City,

                    ArrivalAirportCode = f.ArrivalAirport.CodeIATA,
                    ArrivalAirportCity = f.ArrivalAirport.City,

                    AircraftNumber = f.Aircraft.AircraftNumber,
                    AircraftCompany = f.Aircraft.AircraftType.Company,
                    AircraftModel = f.Aircraft.AircraftType.Model
                })
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();

            return View(flights);
        }
    }
}
