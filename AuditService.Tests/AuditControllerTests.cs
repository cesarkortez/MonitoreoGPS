using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StackExchange.Redis;

using MonitoreoGPS.Shared.Models;  // VehicleCoordinate, RouteHistory

namespace AuditService.Tests
{
    public class AuditControllerTests
    {
        private readonly AuditDbContext _db;
        private readonly Mock<IConnectionMultiplexer> _mockRedis;
        private readonly AuditController _controller;

        public AuditControllerTests()
        {
            // --- 1) Configurar EF Core In‑Memory ---
            var opts = new DbContextOptionsBuilder<AuditDbContext>()
                           .UseInMemoryDatabase("TestDb")
                           .Options;
            _db = new AuditDbContext(opts);

            // --- 2) Crear mock de Redis ---
            _mockRedis = new Mock<IConnectionMultiplexer>();
            var mockDb = new Mock<IDatabase>();
            _mockRedis
                .Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(mockDb.Object);

            // --- 3) Instanciar el controller con db + redis ---
            _controller = new AuditController(_db, _mockRedis.Object);
        }

        [Fact]
        public async Task PostCoord_AddsToDatabase()
        {
            // Arrange
            var coord = new VehicleCoordinate("V1", 4.65, -74.05, DateTime.UtcNow);

            // Act
            var result = await _controller.PostCoord(coord);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Single(_db.Coordinates);
        }

        [Fact]
        public async Task PostRoute_AddsRouteHistory()
        {
            // Arrange
            var route = new RouteHistory
            {
                VehicleId = "V1",
                RouteJson = "[]",
                Timestamp = DateTime.UtcNow
            };

            // Act
            var result = await _controller.PostRoute(route);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Single(_db.Routes);
        }
    }
}


