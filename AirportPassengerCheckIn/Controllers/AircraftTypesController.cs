using AirportPassengerCheckIn.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using AirportPassengerCheckIn.Models.Identity;
using Microsoft.EntityFrameworkCore;
using AirportPassengerCheckIn.DTOs.ViewModels.Aircrafts;
using AirportPassengerCheckIn.Models;

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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AircraftTypeViewModel requestModel)
        {

            if (!ModelState.IsValid)
            {
                return View(requestModel);
            }

            var aircraftCompany = requestModel.Company.Trim();
            var aircraftModel = requestModel.Model.Trim();

            bool exists = await _context.AircraftTypes.AnyAsync(
                at => at.Company == aircraftCompany &&
                at.Model == aircraftModel);

            if (exists)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Such AircraftType is already exists");
                return View(requestModel);
            }

            var aircraftType = new AircraftType
            {
                Company = aircraftCompany,
                Model = aircraftModel,
                NumberOfSeats = requestModel.NumberOfSeats
            };

            _context.AircraftTypes.Add(aircraftType);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "AircraftType has added completly";

            return RedirectToAction(nameof(Index));
        }   
    }
}
