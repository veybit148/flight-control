using FlightControl.Dal.Entities;

namespace FlightControl.Repository.Interfaces;

public interface IPermitCheckResultRepository : IBaseRepository<PermitCheckResult>
{
    Task<IReadOnlyList<PermitCheckResult>> GetByPermitIdsAsync(IEnumerable<Guid> permitIds);
}