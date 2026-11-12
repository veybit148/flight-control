using FlightControl.Dto.Fleets;

namespace FlightControl.Domain.Operations.Fleets;

public interface IAircraftOperation
{
    Task<AircraftResponse> CreateAsync(CreateAircraftRequest request);
    Task<IReadOnlyList<AircraftResponse>> ListAsync(AircraftQuery query);
    Task<AircraftResponse> GetByIdAsync(Guid id);
    Task<AircraftResponse> UpdateAsync(Guid id, UpdateAircraftRequest request);
    Task<AircraftResponse> ChangeStatusAsync(Guid id, ChangeAircraftStatusRequest request);
    Task DeleteAsync(Guid id);
}