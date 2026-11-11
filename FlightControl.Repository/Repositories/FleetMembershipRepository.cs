using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightControl.Repository.Repositories;

public class FleetMembershipRepository : BaseRepository<FleetMembership>, IFleetMembershipRepository
{
    public FleetMembershipRepository(AppDbContext context): base(context){}

    public async Task<IReadOnlyList<FleetMembership>> GetListByFleetIdAsync(Guid fleetId)
    {
        return await _dbSet
            .Where(fm => fm.FleetId == fleetId)
            .ToListAsync();
    }
    public async Task<bool> ExistsAtFleetAsync(Guid fleetId, Guid pilotId)
    {
        return await _dbSet.AnyAsync(fm => fm.PilotId == pilotId && fm.FleetId == fleetId);
    }
    public async Task<FleetMembership?> GetByFleetAndPilotAsync(Guid fleetId, Guid pilotId)
    {
        return await _dbSet.FirstOrDefaultAsync(fm => fm.FleetId == fleetId && fm.PilotId == pilotId);
    }
}
