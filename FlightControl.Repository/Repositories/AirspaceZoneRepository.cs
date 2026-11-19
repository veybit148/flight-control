using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FlightControl.Repository.Repositories;

public class AirspaceZoneRepository : BaseRepository<AirspaceZone>, IAirspaceZoneRepository
{
    public AirspaceZoneRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AirspaceZone>> GetActiveAtPointAsync(Point point, double altitudeMeters, DateTime at)
    {
        return await _dbSet
            .Where(z => z.Area.Covers(point))
            .Where(z => z.MinAltitudeMeters <= altitudeMeters && altitudeMeters <= z.MaxAltitudeMeters)
            .Where(z => z.ActiveFrom == null || z.ActiveFrom <= at)
            .Where(z => z.ActiveUntil == null || at < z.ActiveUntil)
            .ToListAsync();
    }
    public async Task<IReadOnlyList<AirspaceZone>> GetActiveIntersectingAsync(Polygon area, DateTime start, DateTime end)
    {
        return await _dbSet
            .Where(z => z.Area.Intersects(area))
            .Where(z => z.ActiveFrom == null || z.ActiveFrom < end)
            .Where(z => z.ActiveUntil == null || z.ActiveUntil > start)
            .ToListAsync();
    }
}