using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightControl.Repository.Repositories;

public class LicenseCategoryRepository : BaseRepository<LicenseCategory>, ILicenseCategoryRepository
{
    public LicenseCategoryRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LicenseCategory>> GetByLicenseIdsAsync(IEnumerable<Guid> licenseIds)
    {
        return await _dbSet
            .Where(c => licenseIds.Contains(c.LicenseId))
            .ToListAsync();
    }
}