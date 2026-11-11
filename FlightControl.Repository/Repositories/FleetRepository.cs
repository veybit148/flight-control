using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightControl.Repository.Repositories;

public class FleetRepository : BaseRepository<Fleet>, IFleetRepository
{
    public FleetRepository(AppDbContext context): base(context){}

    public async Task<IReadOnlyList<Fleet>> GetListByOwnerIdAsync(Guid ownerId)
    {
        return await _dbSet
            .Where(f => f.OwnerId == ownerId)
            .ToListAsync();
    }
    public async Task<bool> ExistsByNameAsync(Guid ownerId, string name)
    {
        return await _dbSet.AnyAsync(f => f.Name == name && f.OwnerId == ownerId);
    }
}
