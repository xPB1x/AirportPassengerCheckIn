using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace AirportPassengerCheckIn.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<AircraftType> AircraftTypes { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<AirportEmployee> AirportEmployees { get; set; }
        public DbSet<Baggage> Baggages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingPassenger> BookingPassengers { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Seat> Seats { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aircraft
            modelBuilder.Entity<Aircraft>(entity =>
            {
                entity.Property(a => a.AircraftNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(a => a.AircraftNumber)
                    .IsUnique();

                entity.HasOne(a => a.AircraftType)
                    .WithMany(t => t.Aircrafts)
                    .HasForeignKey(a => a.AircraftTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AircraftType
            modelBuilder.Entity<AircraftType>(entity =>
            {
                entity.Property(a => a.Company)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(a => a.Model)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(a => new { a.Company, a.Model })
                    .IsUnique();
            });

            // Airport
            modelBuilder.Entity<Airport>(entity =>
            {
                entity.Property(a => a.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(a => a.CodeIATA)
                    .HasMaxLength(3)
                    .IsRequired();

                entity.HasIndex(a => a.CodeIATA)
                    .IsUnique();
            });

            // AirportEmployee
            modelBuilder.Entity<AirportEmployee>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.MiddleName)
                    .HasMaxLength(100);

                entity.Property(e => e.Position)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            // Baggage
            modelBuilder.Entity<Baggage>(entity =>
            {
                entity.Property(b => b.Weight)
                    .HasPrecision(6, 2);

                entity.HasOne(b => b.Registration)
                    .WithMany(r => r.Baggages)
                    .HasForeignKey(b => b.RegistrationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasIndex(b => b.BookingNumber)
                    .IsUnique();

                entity.Property(b => b.BookingNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasOne(b => b.Flight)
                    .WithMany(f => f.Bookings)
                    .HasForeignKey(b => b.FlightId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //BookingPassenger
            modelBuilder.Entity<BookingPassenger>(entity =>
            {
                entity.HasIndex(bp => new { bp.BookingId, bp.PassengerId })
                    .IsUnique();

                entity.HasOne(bp => bp.Booking)
                    .WithMany(b => b.BookingPassengers)
                    .HasForeignKey(bp => bp.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bp => bp.Passenger)
                    .WithMany(p => p.BookingPassengers)
                    .HasForeignKey(bp => bp.PassengerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bp => bp.Registration)
                    .WithOne(r => r.BookingPassenger)
                    .HasForeignKey<Registration>(r => r.BookingPassengerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Flight
            modelBuilder.Entity<Flight>(entity =>
            {
                entity.Property(f => f.FlightNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasOne(f => f.DepartureAirport)
                    .WithMany()
                    .HasForeignKey(f => f.DepartureAirportId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.ArrivalAirport)
                    .WithMany()
                    .HasForeignKey(f => f.ArrivalAirportId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Aircraft)
                    .WithMany(a => a.Flights)
                    .HasForeignKey(f => f.AircraftId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Passenger
            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.MiddleName).HasMaxLength(100);
                entity.Property(p => p.BirthDate).IsRequired();
                entity.Property(p => p.PassportNumber).IsRequired().HasMaxLength(30);
            });

            // Registration
            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasIndex(r => r.BookingPassengerId)
                    .IsUnique();

                entity.Property(r => r.Type)
                    .HasConversion<string>();

                entity.Property(r => r.Status)
                    .HasConversion<string>();

                entity.HasOne(r => r.Seat)
                    .WithMany()
                    .HasForeignKey(r => r.SeatId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.AirportEmployee)
                    .WithMany(e => e.Registrations)
                    .HasForeignKey(r => r.AirportEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //Seat
            modelBuilder.Entity<Seat>(entity =>
            {
                entity.Property(s => s.SeatNumber)
                    .HasMaxLength(5)
                    .IsRequired();

                entity.HasIndex(s => new { s.AircraftId, s.SeatNumber })
                    .IsUnique();

                entity.HasOne(s => s.Aircraft)
                    .WithMany(a => a.Seats)
                    .HasForeignKey(s => s.AircraftId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

