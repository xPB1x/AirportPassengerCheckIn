using Microsoft.AspNetCore.Identity;
using AirportPassengerCheckIn.Models.Identity;

namespace AirportPassengerCheckIn.Data
{
    public static class IdentityInitializer
    {
        public static async Task InitializeAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            string[] roles =
            {
                "Passenger",
                "AirportEmployee",
                "Admin"
            };

            foreach(var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role)
                    );

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Не удалось создать роль {role}");
                    }
                }
            }

            var adminEmail = configuration["InitialAdmin:Email"];
            var adminPassword = configuration["InitialAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                throw new InvalidOperationException("Email for Admin not found");
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                if (string.IsNullOrWhiteSpace(adminPassword))
                {
                    throw new InvalidOperationException("Password for Admin not found");
                }

                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException("Error when trying to create Admin");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(admin, "Admin");

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Error when trying to add Admin role");
                }
            }
            if (await userManager.IsInRoleAsync(admin, "Passenger"))
            {
                var removeResult = await userManager.RemoveFromRoleAsync(admin, "Passenger");
                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException("Error when trying to remove passenger role from admin");
                }
            }
        }
    }
}
