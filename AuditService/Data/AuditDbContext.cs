using Microsoft.EntityFrameworkCore;
using MonitoreoGPS.Shared.Models;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> opts)
        : base(opts)
    { }

    public DbSet<VehicleCoordinate> Coordinates { get; set; }
    public DbSet<RouteHistory> Routes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aquí definimos la PK compuesta para VehicleCoordinate
        modelBuilder.Entity<VehicleCoordinate>()
            .HasKey(vc => new { vc.VehicleId, vc.Timestamp });

        base.OnModelCreating(modelBuilder);
    }
}

public class RouteHistory
{
    public int Id { get; set; }
    public string VehicleId { get; set; }
    public string RouteJson { get; set; }
    public DateTime Timestamp { get; set; }
}

