using FlightControl.Dal.Entities;
using FlightControl.Repository.Filters;
using NetTopologySuite.Geometries;

namespace FlightControl.Repository.Interfaces;

public interface IFlightPermitRequestRepository : IBaseRepository<FlightPermitRequest>
{
    Task<(IReadOnlyList<FlightPermitRequest> Items, int TotalCount)> GetPagedAsync(PermitFilter filter);
    Task<IReadOnlyList<FlightPermitRequest>> GetApprovedOverlappingAsync(
    Guid excludeId, DateTime start, DateTime end, Polygon? area = null);
    Task<bool> ExistsByAircraftIdAsync(Guid aircraftId);
    Task<bool> ExistsByLicenseIdAsync(Guid licenseId);
}