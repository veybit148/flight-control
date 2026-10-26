using FlightControl.Dal.Entities;

namespace FlightControl.Repository.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string mail);
    Task<bool> ExistsByEmailAsync(string mail);
}