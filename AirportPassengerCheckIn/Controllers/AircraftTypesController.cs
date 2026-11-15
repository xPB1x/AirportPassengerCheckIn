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
        [ValidateAntiForgeryToken]
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var aircraft = await _context.AircraftTypes.FirstOrDefaultAsync(at => at.Id == id);

            if (aircraft == null)
            {
                return NotFound();
            }

            var model = new AircraftTypeViewModel
            {
                Id = aircraft.Id,
                Company = aircraft.Company,
                Model = aircraft.Model,
                NumberOfSeats = aircraft.NumberOfSeats
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AircraftTypeViewModel requestModel)
        {
            if (!ModelState.IsValid)
            {
                return View(requestModel);
            }

            var aircraftType = await _context.AircraftTypes.FirstOrDefaultAsync(at => at.Id == requestModel.Id);
            if (aircraftType == null)
            {
                return NotFound();
            }

            var aircraftCompany = requestModel.Company;
            var aircraftModel = requestModel.Model;

            bool exists = await _context.AircraftTypes.AnyAsync(at =>
                at.Id != requestModel.Id &&
                at.Company == requestModel.Company &&
                at.Model == requestModel.Model);

            if (exists)
            {
                ModelState.AddModelError(
                    string.Empty, "Aircraft Type with such company and model is already exists");
                return View(requestModel);
            }

            aircraftType.Company = aircraftCompany;
            aircraftType.Model = aircraftModel;
            aircraftType.NumberOfSeats = requestModel.NumberOfSeats;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "AircraftType has changed completly";
            return RedirectToAction(nameof(Index));
        }
    }
}
