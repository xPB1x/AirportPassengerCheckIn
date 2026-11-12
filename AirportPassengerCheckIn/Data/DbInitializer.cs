using Microsoft.EntityFrameworkCore;
using AirportPassengerCheckIn.Models;

namespace AirportPassengerCheckIn.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            //Block1 Entities without relationships
            if (!await context.Airports.AnyAsync())
            {
                var airports = new List<Airport>
                {
                    new Airport { Name = "Международный аэропорт Шереметьево", CodeIATA = "SVO", Country = "Россия", City = "Москва" },
                    new Airport { Name = "Международный аэропорт Внуково", CodeIATA = "VKO", Country = "Россия", City = "Москва" },
                    new Airport { Name = "Международный аэропорт Сочи", CodeIATA = "AER", Country = "Россия", City = "Сочи" },
                    new Airport { Name= "Аэропорт Пулково", CodeIATA = "LED", Country = "Россия", City = "Санкт-Петербург" },
                };
                await context.Airports.AddRangeAsync(airports);
                await context.SaveChangesAsync();
            }
            

            if (!await context.AircraftTypes.AnyAsync())
            {
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



            // Block2 Entities with relationships on Block1 Entities

            var boeing737800 = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "737-800");
            var boeing737MAX = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "737 MAX");
            var boeing777300ER = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "777-300ER");
            var airbusA320100 = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "A320-100");
            var airbusA320200 = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "A320-200");

            if (!await context.Aircrafts.AnyAsync())
            {
                var aircrafts = new List<Aircraft>
                {
                    new Aircraft { AircraftNumber = "RA-00001", AircraftType = boeing737800 },
                    new Aircraft { AircraftNumber = "RA-00002", AircraftType = boeing737MAX },
                    new Aircraft { AircraftNumber = "RA-00003", AircraftType = boeing777300ER },
                    new Aircraft { AircraftNumber = "RA-00004", AircraftType = airbusA320100 },
                    new Aircraft { AircraftNumber = "RA-00005", AircraftType = airbusA320200 },
                };
                await context.Aircrafts.AddRangeAsync(aircrafts);
            }


        }
    }
}
