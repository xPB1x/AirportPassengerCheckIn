using Microsoft.AspNetCore.Mvc;
using AirportPassengerCheckIn.Data;

namespace AirportPassengerCheckIn.Controllers
{
    public class FlightsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FlightsController(ApplicationDbContext context)
        {
            _context = context;
        }


    }
}
