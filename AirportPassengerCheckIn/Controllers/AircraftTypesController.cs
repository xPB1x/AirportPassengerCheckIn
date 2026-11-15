using AirportPassengerCheckIn.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using AirportPassengerCheckIn.Models.Identity;
using Microsoft.EntityFrameworkCore;
using AirportPassengerCheckIn.DTOs.ViewModels.Aircrafts;

namespace AirportPassengerCheckIn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AircraftTypesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AircraftTypesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var aircraftTypes = await _context.AircraftTypes
                .Include(at => at.Aircrafts)
                .OrderBy(at => at.Company)
                .ThenBy(at => at.Model)
                .Select(at => new AircraftTypeViewModel
                {
                    Id = at.Id,
                    Company = at.Company,
                    Model = at.Model,
                    NumberOfSeats = at.NumberOfSeats,
                    AircraftCount = at.Aircrafts.Count
                })
                .ToListAsync();

            return View(aircraftTypes);
        }
    }
}
