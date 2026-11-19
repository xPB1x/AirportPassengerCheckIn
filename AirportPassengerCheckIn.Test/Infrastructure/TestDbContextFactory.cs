using AirportPassengerCheckIn.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Tests.Infrastructure
{
    public class TestDbContextFactory : IDisposable
    {
        private readonly SqliteConnection _connection;

        public TestDbContextFactory()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
        }

        public ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new ApplicationDbContext(options);
        }

        public async Task InitializeAsync()
        {
            await using var context = CreateContext();

            await context.Database.EnsureCreatedAsync();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
