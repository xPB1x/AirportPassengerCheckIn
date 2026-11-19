using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Services.Registration;
using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Test.Infrastructure;
using AirportPassengerCheckIn.Tests.Infrastructure;

namespace AirportPassengerCheckIn.Test.Services.Registration.Online
{
    [TestFixture]
    internal class CheckStartRegistrationTests
    {
        [TestCase(12, OnlineRegistrationError.None)]
        [TestCase(25, OnlineRegistrationError.RegistrationWindowClosed)]
        [TestCase(-1, OnlineRegistrationError.RegistrationWindowClosed)]
        [TestCase(0, OnlineRegistrationError.RegistrationWindowClosed)]
        [TestCase(24, OnlineRegistrationError.None)]
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
        [Test]
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

            var result = await service.CheckStartRegistrationAsync("another-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(OnlineRegistrationError.NotFound));
        }

        [Test]
        public async Task CheckStartRegistrationAsync_BookingCanceld_ShouldReturnsBookingNotConfirmed()
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero);
            var timeProvider = new FakeTimeProvider(now);

            var departureTime = now.UtcDateTime.AddHours(12);

            var bookingPassenger = await TestDataFactory.CreateBookingPassengerAsync(context, departureTime);
            bookingPassenger.Booking.Status = BookingStatus.Cancelled;

            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.CheckStartRegistrationAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(OnlineRegistrationError.BookingNotConfirmed));
        }

        [Test]
        public async Task CheckStartRegistrationAsync_RegistrationExists_ShouldReturnsRegistrationAlreadyActive()
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero);
            var timeProvider = new FakeTimeProvider(now);

            var departureTime = now.UtcDateTime.AddHours(12);

            var bookingPassenger = await TestDataFactory.CreateBookingPassengerAsync(context, departureTime);
            var registration = new Models.Registration
            {
                BookingPassengerId = bookingPassenger.Id,
                RegistrationTime = DateTime.UtcNow,
                Type = RegistrationType.Online,
                Status = RegistrationStatus.Active
            };
            bookingPassenger.Registration = registration;

            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.CheckStartRegistrationAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(OnlineRegistrationError.RegistrationAlreadyActive));
        }
    }
}
