using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightControl.Repository.Repositories;

public class PermitCheckResultRepository : BaseRepository<PermitCheckResult>, IPermitCheckResultRepository
{
    public PermitCheckResultRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PermitCheckResult>> GetByPermitIdsAsync(IEnumerable<Guid> permitIds)
    {
        return await _dbSet
            .Where(r => permitIds.Contains(r.PermitRequestId))
            .ToListAsync();
    }
}