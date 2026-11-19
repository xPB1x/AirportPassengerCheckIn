using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Models.Identity;

namespace AirportPassengerCheckIn.Test.Infrastructure
{
    public static class TestDataFactory
    {
        public static async Task<BookingPassenger> CreateBookingPassengerAsync(
            ApplicationDbContext context,
            DateTime departureTime,
            string userId = "test-user")
        {
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = userId,
                NormalizedUserName = userId.ToUpperInvariant(),
                Email = $"{userId}@test.com",
            };

            var departureAirport = new Airport
            {
                Name = "Airport 1",
                CodeIATA = "AAA",
                Country = "Test Country",
                City = "City 1",
                TimeZoneId = "UTC"
            };

            var arrivalAirport = new Airport
            {
                Name = "Airport 2",
                CodeIATA = "BBB",
                Country = "Test Country",
                City = "City 2",
                TimeZoneId = "UTC"
            };

            var aircraftType = new AircraftType
            {
                Company = "Test Company",
                Model = "Test Model",
                NumberOfSeats = 100
            };

            var aircraft = new Aircraft
            {
                AircraftNumber = "TEST-001",
                AircraftType = aircraftType
            };

            var flight = new Flight
            {
                FlightNumber = "TEST-100",
                DepartureTime = departureTime,
                ArrivalTime = departureTime.AddHours(3),
                DepartureAirport = departureAirport,
                ArrivalAirport = arrivalAirport,
                Aircraft = aircraft,
                MaxBaggageWeight = 23
            };

            var passenger = new Passenger
            {
                FirstName = "Ivan",
                LastName = "Smirnov",
                MiddleName = "Alekseevich",
                BirthDate = new DateOnly(2006, 8, 17),
                PassportNumber = "TEST123456"
            };

            var booking = new Booking
            {
                User = user,
                BookingNumber = "TESTBOOKING001",
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Confirmed,
                Flight = flight
            };

    
            var bookingPassenger = new BookingPassenger
            {
                Booking = booking,
                Passenger = passenger
            };

            context.BookingPassengers.Add(bookingPassenger);

            await context.SaveChangesAsync();

            return bookingPassenger;
        }
    }
}
