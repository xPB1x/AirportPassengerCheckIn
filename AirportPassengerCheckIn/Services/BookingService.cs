using AirportPassengerCheckIn.Data;
using Microsoft.EntityFrameworkCore;
using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.DTOs.Bookings;
using AirportPassengerCheckIn.Models;


namespace AirportPassengerCheckIn.Services
{
    public class BookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly BookingNumberGenerator _numberGenerator;
        public BookingService(ApplicationDbContext context,
                              BookingNumberGenerator generator)
        {
            _context = context;
            _numberGenerator = generator;
        }

        public async Task<CreateBookingResponse> CreateBookingAsync(
            CreateBookingRequest request,
            string userId)
        {
            if (request.Passengers == null || request.Passengers.Count == 0)
            {
                throw new InvalidOperationException("Empty Booking");
            }

            var passportNumbers = request.Passengers
                .Select(p => p.PassportNumber.Trim())
                .ToList();

            if (passportNumbers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != passportNumbers.Count)
            {
                throw new InvalidOperationException("There shouldnt be identical passpots");
            }

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                    .ThenInclude(a => a.Seats)
                .FirstOrDefaultAsync(f => f.Id == request.FlightId);

            if (flight == null)
            {
                throw new InvalidOperationException("Flight is not found");
            }

            if (flight.DepartureTime < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Flight has already left");
            }

            int totalSeats = flight.Aircraft.Seats.Count;

            int bookingPassengers = await _context.Bookings
                .Where(b => b.FlightId == flight.Id && b.Status != BookingStatus.Cancelled)
                .SelectMany(b => b.BookingPassengers)
                .CountAsync();

            if (bookingPassengers + request.Passengers.Count > totalSeats)
            {
                throw new InvalidOperationException("Not enough available seats");
            }

            var booking = new Booking
            {
                UserId = userId,
                BookingNumber = await _numberGenerator.GenerateAsync(),
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending,
                FlightId = flight.Id
            };

            foreach (var passengerRequest in request.Passengers)
            {
                string passportNumber = passengerRequest.PassportNumber.Trim();

                var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.PassportNumber == passportNumber);
                if (passenger == null)
                {
                    passenger = new Passenger
                    {
                        FirstName = passengerRequest.FirstName.Trim(),
                        LastName = passengerRequest.LastName.Trim(),
                        MiddleName = passengerRequest.MiddleName?.Trim(),
                        BirthDate = passengerRequest.BirthDate!.Value,
                        PassportNumber = passportNumber
                    };

                    _context.Passengers.Add(passenger);
                }

                booking.BookingPassengers.Add(new BookingPassenger
                {
                    Passenger = passenger
                });
            }
            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            return new CreateBookingResponse
            {
                Id = booking.Id,
                BookingNumber = booking.BookingNumber,
                CreatedAt = booking.CreatedAt,
                Status = booking.Status.ToString(),
                FlightId = booking.FlightId,
                PassengerCount = booking.BookingPassengers.Count
            };
        }

        public async Task<bool> CancelBookingAsync(string bookingNumber)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingPassengers)
                    .ThenInclude(bp => bp.Registration)
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

            if (booking == null || booking.Status == BookingStatus.Cancelled)
            {
                return false;
            }

            booking.Status = BookingStatus.Cancelled;

            foreach(var bookingPassenger in booking.BookingPassengers)
            {
                var registration = bookingPassenger.Registration;
                if (registration == null || 
                    registration.Status == RegistrationStatus.Canceled)
                {
                    continue;
                }

                registration.Status = RegistrationStatus.Canceled;
                registration.CancellationDateTime = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
