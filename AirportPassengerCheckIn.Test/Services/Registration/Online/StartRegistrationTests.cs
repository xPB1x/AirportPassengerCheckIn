using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Services.Registration;
using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Test.Infrastructure;
using AirportPassengerCheckIn.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Test.Services.Registration.Online
{
    [TestFixture]
    internal class StartRegistrationTests
    {
        [Test]
        public async Task StartRegistrationAsync_ValidBooking_ShouldCreatesRegistration()
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(
            2026, 11, 19, 10, 0, 0, TimeSpan.Zero);

            var timeProvider = new FakeTimeProvider(now);

            var bookingPassenger =
                await TestDataFactory.CreateBookingPassengerAsync(
                    context,
                    now.UtcDateTime.AddHours(12));

            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.StartRegistrationAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Succeeded, Is.True);

            var registration = await context.Registrations.FirstOrDefaultAsync(r => r.BookingPassengerId == bookingPassenger.Id);

            Assert.That(registration, Is.Not.Null);
            Assert.That(registration.Status, Is.EqualTo(RegistrationStatus.Active));
            Assert.That(registration.Type, Is.EqualTo(RegistrationType.Online));
        }

        [Test]
        public async Task StartRegistrationAsync_InvalidBookingId_ShouldReturnsNotFound()
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero);
            var timeProvider = new FakeTimeProvider(now);

            var departureTime = now.UtcDateTime.AddHours(12);

            var bookingPassenger = await TestDataFactory.CreateBookingPassengerAsync(context, departureTime);
            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.StartRegistrationAsync("test-user", bookingPassenger.Id+1);

            Assert.That(result.Error, Is.EqualTo(RegistrationError.NotFound));
        }
        [Test]
        public async Task StartRegistrationAsync_OtherUser_ShouldReturnsNotFound()
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero);
            var timeProvider = new FakeTimeProvider(now);

            var departureTime = now.UtcDateTime.AddHours(12);

            var bookingPassenger = await TestDataFactory.CreateBookingPassengerAsync(context, departureTime);
            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.StartRegistrationAsync("another-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(RegistrationError.NotFound));
        }

        [Test]
        public async Task StartRegistrationAsync_BookingCanceld_ShouldReturnsBookingNotConfirmed()
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(
            2026, 11, 19, 10, 0, 0, TimeSpan.Zero);

            var timeProvider = new FakeTimeProvider(now);

            var bookingPassenger =
                await TestDataFactory.CreateBookingPassengerAsync(
                    context,
                    now.UtcDateTime.AddHours(12));
            bookingPassenger.Booking.Status = BookingStatus.Cancelled;

            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.StartRegistrationAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(RegistrationError.BookingNotConfirmed));
        }

        [TestCase(12, RegistrationError.None)]
        [TestCase(25, RegistrationError.RegistrationWindowClosed)]
        [TestCase(-1, RegistrationError.RegistrationWindowClosed)]
        [TestCase(0, RegistrationError.RegistrationWindowClosed)]
        [TestCase(24, RegistrationError.None)]
        public async Task StartRegistrationAsync_ChecksRegistrationWindow(
            int hoursUntilDeparture,
            RegistrationError expectedError)
        {
            using var factory = new TestDbContextFactory();
            await factory.InitializeAsync();
            await using var context = factory.CreateContext();

            var now = new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero);
            var timeProvider = new FakeTimeProvider(now);

            var departureTime = now.UtcDateTime.AddHours(hoursUntilDeparture);

            var bookingPassenger = await TestDataFactory.CreateBookingPassengerAsync(context, departureTime);
            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.StartRegistrationAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(expectedError));
        }

        [Test]
        public async Task StartRegistrationAsync_RegistrationExists_ShouldReturnsRegistrationAlreadyExists()
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

            var result = await service.StartRegistrationAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(RegistrationError.RegistrationAlreadyExists));
        }
    }
}
