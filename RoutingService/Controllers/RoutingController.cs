using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using MonitoreoGPS.Shared.Models;

namespace RoutingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutingController : ControllerBase
    {
        private readonly IDatabase _redis;

        public RoutingController(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VehicleCoordinate coordinate)
        {
            var zoneKey = $"zone:{coordinate.VehicleId}";
            var routeKey = $"route:{coordinate.VehicleId}";

            // Intentar bloquear la zona
            var locked = await _redis.StringSetAsync(
                zoneKey,
                "locked",
                TimeSpan.FromMinutes(5), // Expira en 5 minutos
                When.NotExists
            );

            if (!locked)
            {
                return StatusCode(423, "Zona ocupada");
            }

            try
            {
                // Generar ruta ficticia de ejemplo
                var route = new VehicleCoordinate[]
                {
                    coordinate,
                    new VehicleCoordinate(coordinate.VehicleId, coordinate.Latitude + 0.001, coordinate.Longitude + 0.001, DateTime.UtcNow.AddSeconds(10))
                };

                var serializedRoute = JsonSerializer.Serialize(route);
                await _redis.StringSetAsync(routeKey, serializedRoute, TimeSpan.FromMinutes(10));

                return Ok(route);
            }
            finally
            {
                // Liberar la zona
                await _redis.KeyDeleteAsync(zoneKey);
            }
        }
    }
}