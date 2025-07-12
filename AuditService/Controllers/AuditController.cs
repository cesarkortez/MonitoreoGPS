using Microsoft.AspNetCore.Mvc;
using MonitoreoGPS.Shared.Models;
using StackExchange.Redis; // Para Redis
using System.Transactions; // Para TransactionScope
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AuditDbContext _db;
    private readonly StackExchange.Redis.IDatabase _redis;

    public AuditController(AuditDbContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis.GetDatabase();
    }

    [HttpPost("coordinate")]
    public async Task<IActionResult> PostCoord(VehicleCoordinate coord)
    {
        _db.Coordinates.Add(coord);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("route")]
    public async Task<IActionResult> PostRoute(RouteHistory route)
    {
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        return Ok();
    }

    // NUEVO: DeleteVehicle
    [HttpDelete("vehicle/{vehicleId}")]
    public async Task<IActionResult> DeleteVehicle(string vehicleId)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        // Borrar en PostgreSQL
        var entity = await _db.Coordinates.FirstOrDefaultAsync(c => c.VehicleId == vehicleId);
        if (entity != null)
        {
            _db.Coordinates.Remove(entity);
            await _db.SaveChangesAsync();
        }

        // Borrar en Redis
        var success = await _redis.KeyDeleteAsync($"route:{vehicleId}");
        if (!success)
        {
            // Si falla en Redis lanzamos error para hacer rollback
            throw new Exception("No se pudo borrar la ruta en Redis");
        }

        tx.Complete();
        return Ok($"Vehículo {vehicleId} eliminado de la base de datos y Redis.");
    }
}
