using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightControl.Repository.Repositories;

public class AircraftRepository : BaseRepository<Aircraft>, IAircraftRepository
{
    public AircraftRepository(AppDbContext context): base(context){}

    public async Task<IReadOnlyList<Aircraft>> GetListByFiltersAsync(
        Guid? ownerId,
        Guid? fleetId, 
        AircraftStatus? status,
        AircraftCategory? category
        )
    {
        IQueryable<Aircraft> query = _dbSet;
        
        if(ownerId != null)
        {
            query = query
                .Where(a => a.OwnerId == ownerId);
        }
        
        if(fleetId != null)
        {
            query = query
                .Where(a => a.FleetId == fleetId);
        }

        if(status != null)
        {
            query = query
                .Where(a => a.Status == status);
        }
        
        if(category != null)
        {
            query = query
                .Where(a => a.Category == category);            
        }
    
        return await query.ToListAsync();
    }
    public async Task<bool> ExistsByRegistrationNumberAsync(string RegistrationNumber)
    
    {
        return await _dbSet.AnyAsync(a => a.RegistrationNumber == RegistrationNumber);
    }
}
