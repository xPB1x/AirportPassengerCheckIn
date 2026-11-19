using AirportPassengerCheckIn.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirportPassengerCheckIn.Test.Infrastructure
{
    [TestFixture]
    public class DatabaseTests
    {
        [Test]
        public async Task Database_CanConnect_ReturnsTrue()
        {
            using var factory = new TestDbContextFactory();

            await factory.InitializeAsync();

            await using var context = factory.CreateContext();

            var canConnect = await context.Database.CanConnectAsync();

            Assert.That(canConnect, Is.True);
        }
    }
}
