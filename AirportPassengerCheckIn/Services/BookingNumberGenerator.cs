using System.Security.Cryptography;
using AirportPassengerCheckIn.Data;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Services
{
    public class BookingNumberGenerator
    {
        private readonly ApplicationDbContext _context;
        public BookingNumberGenerator(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<string> GenerateAsync()
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int length = 8;

            while (true)
            {
                var characters = new char[length];
                for (int i = 0; i < length; i++)
                {
                    characters[i] = alphabet[RandomNumberGenerator.GetInt32(0, alphabet.Length)];
                }
                var number = "BK-" + new string(characters);
                bool exists = await _context.Bookings.AnyAsync(b => b.BookingNumber == number);

                if (!exists)
                {
                    return number;
                }
            }
        }
    }
}
