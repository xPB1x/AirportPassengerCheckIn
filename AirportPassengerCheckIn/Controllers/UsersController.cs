using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.Models.Identity;
using AirportPassengerCheckIn.DTOs.ViewModels;

namespace AirportPassengerCheckIn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Include(u => u.AirportEmployee)
                .OrderBy(u => u.AirportEmployeeId)
                .ToListAsync();

            var model = new UserListViewModel();

            foreach(var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Users.Add(new UserListItemViewModel
                {
                    userId = user.Id,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList(),
                    IsLocked = user.LockoutEnabled &&
                               user.LockoutEnd.HasValue &&
                               user.LockoutEnd > DateTimeOffset.UtcNow,
                    EmployeeFullName = user.AirportEmployee == null
                        ? null
                        : $"{user.AirportEmployee.LastName} " +
                          $"{user.AirportEmployee.FirstName} " +
                          $"{user.AirportEmployee.MiddleName}".Trim(),
                    Position = user.AirportEmployee?.Position
                });
            }
            return View(model);
        }
    }
}
