using Microsoft.AspNetCore.Mvc;
using AirportPassengerCheckIn.Data;
using AirportPassengerCheckIn.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{bookingNumber}")]
        public async Task<ActionResult<BookingDto>> GetBooking(string bookingNumber)
        {
            var booking = await _context.Bookings
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.Aircraft)
                        .ThenInclude(a => a.AircraftType)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.Passenger)
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.Registration)
                        .ThenInclude(r => r.Seat)
                .Where(b => b.BookingNumber == bookingNumber)
                .Select(b => new BookingDto
                {
                    BookingNumber = b.BookingNumber,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt,
                    Flight = new FlightDto
                    {
                        Id = b.Flight.Id,
                        FlightNumber = b.Flight.FlightNumber,
                        DepartureTime = b.Flight.DepartureTime,
                        ArrivalTime = b.Flight.ArrivalTime,
                        DepartureAirport = b.Flight.DepartureAirport.CodeIATA,
                        ArrivalAirport = b.Flight.ArrivalAirport.CodeIATA,
                        AircraftNumber = b.Flight.Aircraft.AircraftNumber,
                        AircraftCompany = b.Flight.Aircraft.AircraftType.Company,
                        AircraftModel = b.Flight.Aircraft.AircraftType.Model
                    },
                    Passengers = b.BookingPassengers.Select(bp => new BookingPassengerDto
                    {
                        FirstName = bp.Passenger.FirstName,
                        LastName = bp.Passenger.LastName,
                        MiddleName = bp.Passenger.MiddleName,
                        PassportNumber = bp.Passenger.PassportNumber,
                        RegistrationStatus = bp.Registration != null ? bp.Registration.Status.ToString() : null,
                        SeatNumber = bp.Registration == null ? null : 
                                        bp.Registration.Seat == null ? null : bp.Registration.Seat.SeatNumber
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (booking == null)
            {
                return NotFound();
            }
            return Ok(booking);
        }
    }
}
