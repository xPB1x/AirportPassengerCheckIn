using Microsoft.EntityFrameworkCore;
using AirportPassengerCheckIn.Models;

namespace AirportPassengerCheckIn.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            if (await context.Airports.AnyAsync())
            {
                return;
            }
            var airports = new List<Airport>
            {
                new Airport { Name = "Международный аэропорт Шереметьево", CodeIATA = "SVO", Country = "Россия", City = "Москва" },
                new Airport { Name = "Международный аэропорт Внуково", CodeIATA = "VKO", Country = "Россия", City = "Москва" },
                new Airport { Name = "Международный аэропорт Сочи", CodeIATA = "AER", Country = "Россия", City = "Сочи" },
                new Airport { Name= "Аэропорт Пулково", CodeIATA = "LED", Country = "Россия", City = "Санкт-Петербург" },
            };
            await context.Airports.AddRangeAsync(airports);

            if (await context.AircraftTypes.AnyAsync())
            {
                return;
            }
            var aircraftTypes = new List<AircraftType>
            {
                new AircraftType { Company = "Boeing", Model = "737-800", NumberOfSeats = 189 },
                new AircraftType { Company = "Boeing", Model = "737 MAX", NumberOfSeats = 220 },
                new AircraftType { Company = "Boeing", Model = "777-300ER", NumberOfSeats = 316},
                new AircraftType { Company = "Airbus", Model = "A320-100", NumberOfSeats = 130 },
                new AircraftType {Company = "Airbus", Model = "A320-200", NumberOfSeats = 150 },
            };
            await context.AircraftTypes.AddRangeAsync(aircraftTypes);



            await context.SaveChangesAsync();
        }
    }
}
