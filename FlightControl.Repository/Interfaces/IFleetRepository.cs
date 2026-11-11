using FlightControl.Dal.Entities;

namespace FlightControl.Repository.Interfaces;

public interface IFleetRepository : IBaseRepository<Fleet>
{
    Task<IReadOnlyList<Fleet>> GetListByOwnerIdAsync(Guid ownerId);
    Task<bool> ExistsByNameAsync(Guid ownerId, string name);
}