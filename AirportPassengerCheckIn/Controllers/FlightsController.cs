using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs.ViewModels.Flights;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateFlightViewModel();
            await AddAiportsAndAircraftsToModel(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFlightViewModel model)
        {
            if (model.DepartureAirportId == model.ArrivalAirportId)
            {
                ModelState.AddModelError(
                    nameof(model.ArrivalAirportId),
                    "Arrival airport and Departure airport should be different");
            }

            if (model.DepartureTime != default &&
                model.ArrivalTime != default &&
                model.DepartureTime >= model.ArrivalTime)
            {
                ModelState.AddModelError(
                    nameof(model.ArrivalTime),
                    "ArrivalTime should be greater than DepartureTime");
            }
            if (model.DepartureAirportId > 0 &&
               !await _context.Airports.AnyAsync(a => a.Id == model.DepartureAirportId))
            {
                ModelState.AddModelError(
                    nameof(model.DepartureAirportId),
                    "This DepartureAirport doesn't exist");
            }

            if (model.ArrivalAirportId > 0 &&
                !await _context.Airports.AnyAsync(a => a.Id == model.ArrivalAirportId))
            {
                ModelState.AddModelError(
                    nameof(model.ArrivalAirportId),
                    "This ArrivalAirport doesn't exist");
            }

            if (model.AircraftId > 0 &&
                !await _context.Aircrafts.AnyAsync(a => a.Id == model.AircraftId))
            {
                ModelState.AddModelError(
                    nameof(model.AircraftId),
                    "Choosen aircraft doesn't exists");
            }

            if (!ModelState.IsValid)
            {
                await AddAiportsAndAircraftsToModel(model);
                return View(model);
            }

            var flightNumber = model.FlightNumber.Trim();
            bool exists = await _context.Flights.AnyAsync(f => f.FlightNumber == flightNumber);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(model.FlightNumber),
                    "Flight with this FlightNumber is already exists");

                await AddAiportsAndAircraftsToModel(model);
                return View(model);
            }

            var flight = new Flight
            {
                FlightNumber = flightNumber,
                DepartureTime = model.DepartureTime,
                ArrivalTime = model.ArrivalTime,
                DepartureAirportId = model.DepartureAirportId,
                ArrivalAirportId = model.ArrivalAirportId,
                AircraftId = model.AircraftId
            };

            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Flight has created completly";

            return RedirectToAction(nameof(Index));
        }

        private async Task AddAiportsAndAircraftsToModel(CreateFlightViewModel model)
        {
            model.Airports = await _context.Airports
                .OrderBy(a => a.City)
                    .ThenBy(a => a.CodeIATA)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.City} - {a.Name} ({a.CodeIATA})"
                })
                .ToListAsync();

            model.Aircrafts = await _context.Aircrafts
                .Include(a => a.AircraftType)
                .OrderBy(a => a.AircraftNumber)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.AircraftNumber} - {a.AircraftType.Company} {a.AircraftType.Model}"
                })
                .ToListAsync();
        }
    }
}
