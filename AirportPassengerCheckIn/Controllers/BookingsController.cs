using Microsoft.AspNetCore.Mvc;
using AirportPassengerCheckIn.Data;

namespace AirportPassengerCheckIn.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }


    }
}
