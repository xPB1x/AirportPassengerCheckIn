using Microsoft.AspNetCore.Mvc;
using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.Models;

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
    }
}
