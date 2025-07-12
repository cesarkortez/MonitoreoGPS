
using Microsoft.AspNetCore.Mvc;
using MonitoreoGPS.Shared.Models;
using MonitoreoGPS.Shared.Specifications;
using StackExchange.Redis;

namespace GeolocationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeolocationController : ControllerBase
    {
        private readonly IDatabase _redis;
        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);
        private readonly ISpecification<VehicleCoordinate> _coordSpec = new CoordinateSpecification();

        public GeolocationController(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        [HttpPost]
        public async Task<IActionResult> Post(VehicleCoordinate coord)
        {
            if (!_coordSpec.IsSatisfiedBy(coord))
                return BadRequest("Coordenadas inválidas.");

            string key = $"{coord.VehicleId}:{coord.Latitude}:{coord.Longitude}";
            // Detección duplicados: SETNX devuelve 1 si se establece
            bool added = await _redis.StringSetAsync(key, Newtonsoft.Json.JsonConvert.SerializeObject(coord), _ttl, When.NotExists);
            if (!added) return Conflict("Duplicado.");

            return Ok();
        }
    }
}
