using FlightControl.Dto.Fleets;

namespace FlightControl.Domain.Operations.Fleets;

public interface IFleetOperation
{
    Task<FleetResponse> CreateAsync(CreateFleetRequest request);
    Task<IReadOnlyList<FleetResponse>> ListAsync();
    Task<FleetResponse> GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task<FleetMemberResponse> AddMemberAsync(Guid fleetId, AddFleetMemberRequest request);
    Task<IReadOnlyList<FleetMemberResponse>> ListMembersAsync(Guid fleetId);
    Task RemoveMemberAsync(Guid fleetId, Guid pilotId);
}