using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.Models.Identity;
using AirportPassengerCheckIn.DTOs.ViewModels;
using AirportPassengerCheckIn.Models;

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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (model.AccountType != "Passenger" &&
                model.AccountType != "AirportEmployee")
            {
                ModelState.AddModelError(
                    nameof(model.AccountType),
                    "Недопустимый тип пользователя");
            }

            if (model.AccountType == "AirportEmployee")
            {
                if (string.IsNullOrWhiteSpace(model.FirstName))
                    ModelState.AddModelError(nameof(model.FirstName), "Укажите имя");

                if (string.IsNullOrWhiteSpace(model.LastName))
                    ModelState.AddModelError(nameof(model.LastName), "Укажите фамилию");

                if (string.IsNullOrWhiteSpace(model.Position))
                    ModelState.AddModelError(nameof(model.Position), "Укажите должность");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int? employeeId = null;
                if (model.AccountType == "AirportEmployee")
                {
                    var employee = new AirportEmployee
                    {
                        FirstName = model.FirstName!.Trim(),
                        LastName = model.LastName!.Trim(),
                        MiddleName = model.MiddleName?.Trim(),
                        Position = model.Position!.Trim()
                    };

                    _context.AirportEmployees.Add(employee);
                    await _context.SaveChangesAsync();

                    employeeId = employee.Id;
                }

                var email = model.Email.Trim();
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    AirportEmployeeId = employeeId,
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty, error.Description);
                    }

                    await transaction.RollbackAsync();
                    return View(model);
                }

                var role = model.AccountType;
                var roleResult = await _userManager.CreateAsync(user, model.AccountType);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty, error.Description);
                    }

                    await transaction.RollbackAsync();
                    return View(model);
                }

                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "The User was completly created";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
