using FlightControl.Dto.Airspace;

namespace FlightControl.Dto.Permits;

public record CreatePermitRequest(
    Guid AircraftId,
    Guid LicenseId,
    IReadOnlyList<CoordinateDto> Area,
    DateTime PlannedStart,
    DateTime PlannedEnd,
    double MaxAltitudeMeters,
    string Purpose
);