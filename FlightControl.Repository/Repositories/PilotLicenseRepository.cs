using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightControl.Repository.Repositories;

public class PilotLicenseRepository : BaseRepository<PilotLicense>, IPilotLicenseRepository
{
    public PilotLicenseRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExistsByLicenseNumberAsync(string licenseNumber)
    {
        return await _dbSet.AnyAsync(l => l.LicenseNumber == licenseNumber);
    }

    public async Task<IReadOnlyList<PilotLicense>> GetListByPilotIdAsync(Guid? pilotId)
    {
        IQueryable<PilotLicense> query = _dbSet;

        if (pilotId != null)
        {
            query = query.Where(l => l.PilotId == pilotId);
        }

        return await query.ToListAsync();
    }
}