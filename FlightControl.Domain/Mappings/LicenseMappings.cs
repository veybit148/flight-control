using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Dto.Licenses;

namespace FlightControl.Domain.Mappings;

public static class LicenseMappings
{
    public static PilotLicense ToEntity(this CreateLicenseRequest request, Guid pilotId) => new()
    {
        PilotId = pilotId,
        LicenseNumber = request.LicenseNumber,
        ExpiresAt = request.ExpiresAt
    };

    public static LicenseResponse ToResponse(
        this PilotLicense license,
        string pilotName,
        IEnumerable<AircraftCategory> categories) => new(
        license.Id,
        license.PilotId,
        pilotName,
        license.LicenseNumber,
        license.ExpiresAt,
        license.Status.ToString(),
        license.SuspensionReason,
        categories.OrderBy(c => c).Select(c => c.ToString()).ToList(),
        license.CreatedAt
    );
}