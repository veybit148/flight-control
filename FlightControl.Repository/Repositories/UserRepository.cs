
using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightControl.Repository.Repositories;

public class UserRepository: BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context): base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {   
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

}