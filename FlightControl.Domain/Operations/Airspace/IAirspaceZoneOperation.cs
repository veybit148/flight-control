using FlightControl.Dto.Airspace;

namespace FlightControl.Domain.Operations.Airspace;

public interface IAirspaceZoneOperation
{
    Task<AirspaceZoneResponse> CreateAsync(AirspaceZoneRequest request);
    Task<IReadOnlyList<AirspaceZoneResponse>> ListAsync();
    Task<AirspaceZoneResponse> GetByIdAsync(Guid id);
    Task<AirspaceZoneResponse> UpdateAsync(Guid id, AirspaceZoneRequest request);
    Task DeleteAsync(Guid id);
    Task<IReadOnlyList<AirspaceZoneResponse>> GetAtPointAsync(ZonesAtPointQuery query);
}