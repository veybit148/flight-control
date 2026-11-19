using FlightControl.Dal.Entities;
using NetTopologySuite.Geometries;

namespace FlightControl.Repository.Interfaces;

public interface IAirspaceZoneRepository : IBaseRepository<AirspaceZone>
{
    Task<IReadOnlyList<AirspaceZone>> GetActiveAtPointAsync(Point point, double altitudeMeters, DateTime at);
    Task<IReadOnlyList<AirspaceZone>> GetActiveIntersectingAsync(Polygon area, DateTime start, DateTime end);
}