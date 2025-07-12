using Microsoft.AspNetCore.Mvc;
using Moq;
using RoutingService.Controllers;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using MonitoreoGPS.Shared.Models;

namespace RoutingService.Tests
{
    public class RoutingControllerTests
    {
        [Fact]
        public async Task Post_ReturnsRoute_WhenZoneIsFree()
        {
            // Arrange
            var redisMock = new Mock<IDatabase>();
            redisMock.Setup(r => r.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            )).ReturnsAsync(true);

            redisMock.Setup(r => r.StringSetAsync(
                It.Is<string>(key => key.StartsWith("route:")),
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            )).ReturnsAsync(true);

            redisMock.Setup(r => r.KeyDeleteAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<CommandFlags>()
            )).ReturnsAsync(true);

            var multiplexerMock = new Mock<IConnectionMultiplexer>();
            multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                           .Returns(redisMock.Object);

            var controller = new RoutingController(multiplexerMock.Object);

            var coord = new VehicleCoordinate(
                "123",              // VehicleId
                10.5,               // Latitude
                -75.3,              // Longitude
                DateTime.UtcNow     // Timestamp
            );

            // Act
            var result = await controller.Post(coord);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var route = Assert.IsAssignableFrom<VehicleCoordinate[]>(okResult.Value);
            Assert.Equal(2, route.Length);
            Assert.Equal(coord.VehicleId, route[0].VehicleId);
        }

        [Fact]
        public async Task Post_Returns423_WhenZoneIsLocked()
        {
            // Arrange
            var redisMock = new Mock<IDatabase>();
            redisMock.Setup(r => r.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            )).ReturnsAsync(false);

            var multiplexerMock = new Mock<IConnectionMultiplexer>();
            multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                           .Returns(redisMock.Object);

            var controller = new RoutingController(multiplexerMock.Object);

            var coord = new VehicleCoordinate(
                "456",              // VehicleId
                20.1,               // Latitude
                -70.9,              // Longitude
                DateTime.UtcNow     // Timestamp
            );

            // Act
            var result = await controller.Post(coord);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(423, objectResult.StatusCode);
            Assert.Equal("Zona ocupada", objectResult.Value);
        }
    }
}
