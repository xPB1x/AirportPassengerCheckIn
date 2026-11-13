using AirportPassengerCheckIn.Data;
using Microsoft.EntityFrameworkCore;
using AirportPassengerCheckIn.Models.Enums;


namespace AirportPassengerCheckIn.Services
{
    public class BookingService
    {
        private readonly ApplicationDbContext _context;
        public BookingService(ApplicationDbContext context)
        {
            _context = context;
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
