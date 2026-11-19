using FlightControl.Dal.Entities;
using FlightControl.Dto.Permits;

namespace FlightControl.Domain.Mappings;

public static class PermitMappings
{
    public static PermitResponse ToResponse(
        this FlightPermitRequest permit,
        string requesterName,
        Aircraft aircraft,
        PilotLicense license,
        string pilotName,
        IEnumerable<PermitCheckResult> checks) => new(
        permit.Id,
        permit.Type.ToString(),
        permit.Status.ToString(),
        permit.RequesterId,
        requesterName,
        permit.AircraftId,
        aircraft.RegistrationNumber,
        permit.LicenseId,
        license.LicenseNumber,
        permit.PilotId,
        pilotName,
        permit.FleetId,
        permit.Area.ToCoordinateDtos(),
        permit.PlannedStart,
        permit.PlannedEnd,
        permit.MaxAltitudeMeters,
        permit.Purpose,
        permit.DecidedById,
        permit.DecidedAt,
        permit.DecisionNote,
        permit.CancelledAt,
        permit.CreatedAt,
        checks
            .OrderBy(c => c.RuleName)
            .Select(c => new PermitCheckDto(c.RuleName, c.Outcome.ToString(), c.Description))
            .ToList()
    );
}