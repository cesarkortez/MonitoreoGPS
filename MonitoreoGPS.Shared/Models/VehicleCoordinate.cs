using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoreoGPS.Shared.Models
{
    public record VehicleCoordinate(string VehicleId, double Latitude, double Longitude, DateTime Timestamp);
}
