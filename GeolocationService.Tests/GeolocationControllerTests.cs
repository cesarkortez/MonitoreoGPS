using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StackExchange.Redis;
using Xunit;
using MonitoreoGPS.Shared.Models;
using GeolocationService.Controllers; 

namespace GeolocationService.Tests
{
    public class GeolocationControllerTests
    {
        private readonly GeolocationController _controller;
        private readonly Mock<IConnectionMultiplexer> _muxMock;
        private readonly Mock<IDatabase> _dbMock;

        public GeolocationControllerTests()
        {
            _muxMock = new Mock<IConnectionMultiplexer>();
            _dbMock = new Mock<IDatabase>();
            _muxMock
                .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_dbMock.Object);

            _controller = new GeolocationController(_muxMock.Object);
        }

        [Fact]
        public async Task Post_NewCoordinate_FirstIsOk_SecondIsConflict()
        {
            // Arrange
            var coord = new VehicleCoordinate("V1", 4.65, -74.05, DateTime.UtcNow);

            // Hacemos que el primer StringSetAsync devuelva true y el segundo false
            _dbMock.SetupSequence(d => d.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan>(),
                    When.NotExists))
                .ReturnsAsync(true)   // primer envío → Ok
                .ReturnsAsync(false); // segundo envío → Conflict

            // Act #1
            var first = await _controller.Post(coord);
            Assert.IsType<OkResult>(first);

            // Act #2
            var second = await _controller.Post(coord);
            var conflict = Assert.IsType<ConflictObjectResult>(second);
            Assert.Equal("Duplicado.", conflict.Value);
        }
    }
}
