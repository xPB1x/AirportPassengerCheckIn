using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.Models.Identity;
using AirportPassengerCheckIn.DTOs.ViewModels;
using AirportPassengerCheckIn.Models;
using System.Diagnostics;

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
                    UserId = user.Id,
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
                TempData["SuccessMessage"] = "The User has completly created";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _context.Users
                .Include(u => u.AirportEmployee)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                Debug.WriteLine("GG");
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                UserId = id,
                Email = user.Email ?? string.Empty,

                IsEmployee = user.AirportEmployee != null,

                FirstName = user.AirportEmployee?.FirstName,
                LastName = user.AirportEmployee?.LastName,
                MiddleName = user.AirportEmployee?.MiddleName,
                Position = user.AirportEmployee?.Position
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.AirportEmployee)
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (existingUser == null || existingUser.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "User with this email has already created");
                return View(model);
            }

            if (user.AirportEmployee != null)
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
                var email = model.Email.Trim();
                user.Email = email;
                user.UserName = email;

                var updateResult = await _userManager.UpdateAsync(user);

                if(!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty, error.Description);
                    }
                    await transaction.RollbackAsync();
                    return View(model);
                }

                if (user.AirportEmployee != null)
                {
                    user.AirportEmployee.FirstName = model.FirstName!.Trim();
                    user.AirportEmployee.LastName = model.LastName!.Trim();
                    user.AirportEmployee.MiddleName = model.MiddleName?.Trim();
                    user.AirportEmployee.Position = model.Position!.Trim();

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "The User's data has updated completly";
                return RedirectToAction(nameof(Index));

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "You can't block your Account";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "You can't block Admin";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.SetLockoutEnabledAsync(user, true);

            var result = await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.MaxValue);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ",
                    result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            await _userManager.UpdateSecurityStampAsync(user);

            TempData["SuccessMessage"] = "Пользователь заблокирован";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "You can't unblock Admin";
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ",
                    result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            await _userManager.UpdateSecurityStampAsync(user);

            TempData["SuccessMessage"] = "The user has unblocked";

            return RedirectToAction(nameof(Index));
        }
    }
}
