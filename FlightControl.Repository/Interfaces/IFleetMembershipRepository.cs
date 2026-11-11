using FlightControl.Dal.Entities;

namespace FlightControl.Repository.Interfaces;

public interface IFleetMembershipRepository : IBaseRepository<FleetMembership>
{
    Task<IReadOnlyList<FleetMembership>> GetListByFleetIdAsync(Guid fleetId);
    Task<bool> ExistsAtFleetAsync(Guid fleetId, Guid pilotId);
    Task<FleetMembership?> GetByFleetAndPilotAsync(Guid fleetId, Guid pilotId);
}