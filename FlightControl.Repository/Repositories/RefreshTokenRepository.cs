using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightControl.Repository.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context): base(context){}

    public async Task<RefreshToken?> GetByHashAsync(string hash)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.TokenHash == hash);       
    }
    
    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
    {   
        return await _dbSet
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();
    }
}