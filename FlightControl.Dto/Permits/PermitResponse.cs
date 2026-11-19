using FlightControl.Dto.Airspace;

namespace FlightControl.Dto.Permits;

public record PermitResponse(
    Guid Id,
    string Type,
    string Status,
    Guid RequesterId,
    string RequesterName,
    Guid AircraftId,
    string AircraftRegistrationNumber,
    Guid LicenseId,
    string LicenseNumber,
    Guid PilotId,
    string PilotName,
    Guid? FleetId,
    IReadOnlyList<CoordinateDto> Area,
    DateTime PlannedStart,
    DateTime PlannedEnd,
    double MaxAltitudeMeters,
    string Purpose,
    Guid? DecidedById,
    DateTime? DecidedAt,
    string? DecisionNote,
    DateTime? CancelledAt,
    DateTime CreatedAt,
    IReadOnlyList<PermitCheckDto> Checks
);