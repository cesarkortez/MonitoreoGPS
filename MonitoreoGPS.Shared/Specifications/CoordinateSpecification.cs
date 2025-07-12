using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoreoGPS.Shared.Specifications
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T item);
    }

    // Ejemplo: validación de coordenadas
    public class CoordinateSpecification : ISpecification<MonitoreoGPS.Shared.Models.VehicleCoordinate>
    {
        public bool IsSatisfiedBy(MonitoreoGPS.Shared.Models.VehicleCoordinate c) =>
            c.Latitude >= -90 && c.Latitude <= 90 &&
            c.Longitude >= -180 && c.Longitude <= 180;
    }
}

