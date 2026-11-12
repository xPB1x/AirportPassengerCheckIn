using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FlightsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<FlightDTO>>> GetFlights()
        {
            var flights = await _context.Flights
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                    .ThenInclude(a => a.AircraftType)
                .Select(f => new FlightDTO
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,

                    DepartureAirport = f.DepartureAirport.CodeIATA,
                    ArrivalAirport = f.ArrivalAirport.CodeIATA,

                    AircraftNumber = f.Aircraft.AircraftNumber,
                    AircraftCompany = f.Aircraft.AircraftType.Company,
                    AircraftModel = f.Aircraft.AircraftType.Model
                })
                .ToListAsync();

            return Ok(flights);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlightDTO>> GetFlight(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Aircraft)
                    .ThenInclude(a => a.AircraftType)
                .Where(f => f.Id == id)
                .Select(f => new FlightDTO
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,

                    DepartureAirport = f.DepartureAirport.CodeIATA,
                    ArrivalAirport = f.ArrivalAirport.CodeIATA,

                    AircraftNumber = f.Aircraft.AircraftNumber,
                    AircraftCompany = f.Aircraft.AircraftType.Company,
                    AircraftModel = f.Aircraft.AircraftType.Model
                })
                .FirstOrDefaultAsync();

            if (flight == null)
            {
                return NotFound();
            }
            return Ok(flight);
        }
    }
}
