using Microsoft.AspNetCore.Identity;

namespace AirportPassengerCheckIn.Data
{
    public static class IdentityInitializer
    {
        public static async Task InitializeAsync(RoleManager<IdentityRole> roleManager)
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
        }
    }
}
