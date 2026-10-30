using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;

namespace FlightControl.Repository.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string mail);
    Task<bool> ExistsByEmailAsync(string mail);
    Task<bool> ExistsByRoleAsync(UserRole mail);
}