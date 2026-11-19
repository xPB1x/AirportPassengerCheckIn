using AirportPassengerCheckIn.Services.Registration;
using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Test.Infrastructure;
using AirportPassengerCheckIn.Tests.Infrastructure;

namespace AirportPassengerCheckIn.Test.Services
{
    [TestFixture]
    internal class OnlineRegistrationServiceTests
    {
        [TestCase(12, OnlineRegistrationError.None)]
        [TestCase(25, OnlineRegistrationError.RegistrationWindowClosed)]
        [TestCase(-1, OnlineRegistrationError.RegistrationWindowClosed)]
        public async Task CheckStartRegistrationAsync_ChecksRegistrationWindow(
            int hoursUntilDeparture,
            OnlineRegistrationError expectedError)
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero);
            var timeProvider = new FakeTimeProvider(now);

            var departureTime = now.UtcDateTime.AddHours(hoursUntilDeparture);

            var bookingPassenger = await TestDataFactory.CreateBookingPassengerAsync(context, departureTime);
            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.CheckStartRegistrationAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(expectedError));
        }

        public async Task CheckStartRegistrationAsync_OtherUser_ShouldReturnsNotFound()
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero);
            var timeProvider = new FakeTimeProvider(now);

            var departureTime = now.UtcDateTime.AddHours(12);

            var bookingPassenger = await TestDataFactory.CreateBookingPassengerAsync(context, departureTime);
            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.CheckStartRegistrationAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(OnlineRegistrationError.NotFound));
        }
    }
}
