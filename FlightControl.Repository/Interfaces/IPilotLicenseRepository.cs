using FlightControl.Dal.Entities;

namespace FlightControl.Repository.Interfaces;

public interface IPilotLicenseRepository : IBaseRepository<PilotLicense>
{
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
    Task<IReadOnlyList<PilotLicense>> GetListByPilotIdAsync(Guid? pilotId);
}