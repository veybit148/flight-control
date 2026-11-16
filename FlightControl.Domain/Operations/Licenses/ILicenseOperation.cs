using FlightControl.Dto.Licenses;

namespace FlightControl.Domain.Operations.Licenses;

public interface ILicenseOperation
{
    Task<LicenseResponse> CreateAsync(CreateLicenseRequest request);
    Task<IReadOnlyList<LicenseResponse>> ListAsync();
    Task<LicenseResponse> GetByIdAsync(Guid id);
    Task<LicenseResponse> UpdateAsync(Guid id, UpdateLicenseRequest request);
    Task DeleteAsync(Guid id);
    Task<LicenseResponse> SuspendAsync(Guid id, SuspendLicenseRequest request);
    Task<LicenseResponse> ReinstateAsync(Guid id);
}