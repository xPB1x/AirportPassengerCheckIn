using AirportPassengerCheckIn.Models.Enums;
using AirportPassengerCheckIn.Services.Registration;
using AirportPassengerCheckIn.Services.Registration.Results;
using AirportPassengerCheckIn.Test.Infrastructure;
using AirportPassengerCheckIn.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AirportPassengerCheckIn.Test.Services.Registration.Online
{
    [TestFixture]
    internal class GetContinueStepOnlineTests
    {
        [Test]
        public async Task GetContinueStepOnlineAsync_ActiveRegistrationWithoutSeat_ShouldReturnSelectSeat()
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

            var registration = new Models.Registration
            {
                BookingPassengerId = bookingPassenger.Id,
                RegistrationTime = DateTime.UtcNow,
                Type = RegistrationType.Online,
                Status = RegistrationStatus.Active
            };
            bookingPassenger.Registration = registration;

            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.GetContinueStepOnlineAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Succeeded, Is.EqualTo(true));
            Assert.That(result.NextStep, Is.EqualTo(RegistrationStep.SelectSeat));
        }

        [Test]
        public async Task GetContinueStepOnlineAsync_ActiveRegistrationWithSeat_ShouldReturnConfirm()
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

            var registration = new Models.Registration
            {
                BookingPassengerId = bookingPassenger.Id,
                RegistrationTime = DateTime.UtcNow,
                Type = RegistrationType.Online,
                Status = RegistrationStatus.Active,
                SeatId = 1
            };

            bookingPassenger.Registration = registration;

            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.GetContinueStepOnlineAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Succeeded, Is.EqualTo(true));
            Assert.That(result.NextStep, Is.EqualTo(RegistrationStep.Confirm));
        }

        [Test]
        public async Task GetContinueStepOnlineAsync_InvalidBookingId_ShouldReturnsNotFound()
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

            var result = await service.GetContinueStepOnlineAsync("test-user", bookingPassenger.Id + 1);

            Assert.That(result.Error, Is.EqualTo(RegistrationError.NotFound));
        }
        [Test]
        public async Task GetContinueStepOnlineAsync_OtherUser_ShouldReturnsNotFound()
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

            var result = await service.GetContinueStepOnlineAsync("another-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(RegistrationError.NotFound));
        }

        [Test]
        public async Task GetContinueStepOnlineAsync_BookingCanceld_ShouldReturnsBookingNotConfirmed()
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
            var registration = new Models.Registration
            {
                BookingPassengerId = bookingPassenger.Id,
                RegistrationTime = DateTime.UtcNow,
                Type = RegistrationType.Online,
                Status = RegistrationStatus.Active
            };
            bookingPassenger.Registration = registration;


            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.GetContinueStepOnlineAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(RegistrationError.BookingNotConfirmed));
        }

        [TestCase(12, RegistrationError.None)]
        [TestCase(25, RegistrationError.RegistrationWindowClosed)]
        [TestCase(-1, RegistrationError.RegistrationWindowClosed)]
        [TestCase(0, RegistrationError.RegistrationWindowClosed)]
        [TestCase(24, RegistrationError.None)]
        public async Task GetContinueStepOnlineAsync_ChecksRegistrationWindow(
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
            var registration = new Models.Registration
            {
                BookingPassengerId = bookingPassenger.Id,
                RegistrationTime = DateTime.UtcNow,
                Type = RegistrationType.Online,
                Status = RegistrationStatus.Active
            };
            bookingPassenger.Registration = registration;


            var service = new OnlineRegistrationService(context, timeProvider);

            var result = await service.GetContinueStepOnlineAsync("test-user", bookingPassenger.Id);

            Assert.That(result.Error, Is.EqualTo(expectedError));
        }
    }
}
