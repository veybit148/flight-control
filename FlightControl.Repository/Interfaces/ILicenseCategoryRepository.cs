using FlightControl.Dal.Entities;

namespace FlightControl.Repository.Interfaces;

public interface ILicenseCategoryRepository : IBaseRepository<LicenseCategory>
{
    Task<IReadOnlyList<LicenseCategory>> GetByLicenseIdsAsync(IEnumerable<Guid> licenseIds);
}