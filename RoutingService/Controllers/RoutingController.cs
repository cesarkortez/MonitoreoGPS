using Microsoft.AspNetCore.Mvc;
using MonitoreoGPS.Shared.Models;
using Polly.CircuitBreaker;
using Polly;
using StackExchange.Redis;
using System.Text.Json;

namespace RoutingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutingController : ControllerBase
    {
        private static readonly AsyncCircuitBreakerPolicy _breaker =
            Policy.Handle<Exception>()
                  .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30)); // Cambiado a 2 fallos

        private readonly IDatabase _redis;

        public RoutingController(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VehicleCoordinate coord)
        {
            try
            {
                return await _breaker.ExecuteAsync<IActionResult>(async () =>
                {
                    string zoneKey = $"{Math.Floor(coord.Latitude)}_{Math.Floor(coord.Longitude)}";

                    bool lockTaken = await _redis.StringSetAsync(
                        "lock:" + zoneKey,
                        "1",
                        TimeSpan.FromSeconds(5),
                        When.NotExists);

                    if (!lockTaken)
                    {
                        return new ObjectResult("Zona ocupada") { StatusCode = 423 };
                    }

                    try
                    {
                        var route = new[] {
                            new VehicleCoordinate(
                                coord.VehicleId,
                                coord.Latitude,
                                coord.Longitude,
                                coord.Timestamp),
                            new VehicleCoordinate(
                                coord.VehicleId,
                                coord.Latitude + 0.01,
                                coord.Longitude + 0.01,
                                coord.Timestamp.AddSeconds(60))
                        };

                        await _redis.StringSetAsync(
                            $"route:{coord.VehicleId}",
                            JsonSerializer.Serialize(route),
                            TimeSpan.FromMinutes(5));

                        return new OkObjectResult(route);
                    }
                    finally
                    {
                        await _redis.KeyDeleteAsync("lock:" + zoneKey);
                    }
                });
            }
            catch (BrokenCircuitException)
            {
                return StatusCode(503, "Circuit Breaker activado");
            }
        }
    }
}