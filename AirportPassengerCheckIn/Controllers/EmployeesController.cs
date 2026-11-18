using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs.ViewModels;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AirportPassengerCheckIn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var employee = new AirportEmployee
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                MiddleName = model.MiddleName?.Trim(),
                Position = model.Position.Trim()
            };

            _context.AirportEmployees.Add(employee);
            await _context.SaveChangesAsync();

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                AirportEmployeeId = employee.Id
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await transaction.RollbackAsync();

                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "AirportEmployee");
            if (!roleResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await transaction.RollbackAsync();

                return View(model);
            }

            await transaction.CommitAsync();
            TempData["SuccededMessage"] = "Employee was completly created";

            return RedirectToAction(nameof(Create));
        }
    }
}
