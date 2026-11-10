using FlightControl.Dal.Entities;

namespace FlightControl.Repository.Interfaces;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByHashAsync(string hash);
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId);
}