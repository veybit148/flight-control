using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Repository.Filters;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FlightControl.Repository.Repositories;

public class FlightPermitRequestRepository : BaseRepository<FlightPermitRequest>, IFlightPermitRequestRepository
{
    public FlightPermitRequestRepository(AppDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<FlightPermitRequest> Items, int TotalCount)> GetPagedAsync(PermitFilter filter)
    {
        IQueryable<FlightPermitRequest> query = _dbSet;

        if(filter.RequesterId != null)
        {
            query = query.Where(p => p.RequesterId == filter.RequesterId);
        }

        if(filter.Status != null)
        {
            query = query.Where(p => p.Status == filter.Status);
        }

        if(filter.Type != null)
        {
            query = query.Where(p => p.Type == filter.Type);
        }

        if(filter.From != null)
        {
            query = query.Where(p => p.PlannedEnd > filter.From);
        }

        if(filter.To != null)
        {
            query = query.Where(p => p.PlannedStart < filter.To);
        }

        int totalCount = await query.CountAsync();

        IOrderedQueryable<FlightPermitRequest> ordered = filter.MilitaryFirst
            ? query.OrderBy(p => p.Type == PermitRequestType.Military ? 0 : 1).ThenBy(p => p.PlannedStart)
            : query.OrderBy(p => p.PlannedStart);

        List<FlightPermitRequest> items = await ordered
            .ThenBy(p => p.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    public async Task<IReadOnlyList<FlightPermitRequest>> GetApprovedOverlappingAsync(
    Guid excludeId, DateTime start, DateTime end, Polygon? area = null)
    {
        IQueryable<FlightPermitRequest> query = _dbSet
            .Where(p => p.Id != excludeId && p.Status == PermitStatus.Approved)
            .Where(p => p.PlannedStart < end && start < p.PlannedEnd);

        if (area != null)
        {
            query = query.Where(p => p.Area.Intersects(area));
        }

        return await query.ToListAsync();
    }

    public async Task<bool> ExistsByAircraftIdAsync(Guid aircraftId)
    {
        return await _dbSet.AnyAsync(p => p.AircraftId == aircraftId);
    }

    public async Task<bool> ExistsByLicenseIdAsync(Guid licenseId)
    {
        return await _dbSet.AnyAsync(p => p.LicenseId == licenseId);
    }
}