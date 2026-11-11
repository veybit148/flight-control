using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;

namespace FlightControl.Repository.Interfaces;

public interface IAircraftRepository : IBaseRepository<Aircraft>
{
    Task<IReadOnlyList<Aircraft>> GetListByFiltersAsync(
        Guid? ownerId,
        Guid? fleetId, 
        AircraftStatus? status,
        AircraftCategory? category
        );
    Task<bool> ExistsByRegistrationNumberAsync(string RegistrationNumber);
}