using AirportPassengerCheckIn.Models;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        DbSet<Aircraft> Aircrafts { get; set; }
        DbSet<AircraftType> AircraftTypes { get; set; }
        DbSet<Airport> Airports { get; set; }
        DbSet<AirportEmployee> AirportEmployees { get; set; }
        DbSet<Baggage> Baggages { get; set; }
        DbSet<Booking> Bookings { get; set; }
        DbSet<BookingPassenger> BookingPassengers { get; set; }
        DbSet<Flight> Flights { get; set; }
        DbSet<Passenger> Passengers { get; set; }
        DbSet<Registration> Registrations { get; set; }
        DbSet<Seat> Seats { get; set; }
    }
}
