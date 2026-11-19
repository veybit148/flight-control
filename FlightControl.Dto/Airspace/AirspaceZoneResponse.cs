namespace FlightControl.Dto.Airspace;

public record AirspaceZoneResponse(
    Guid Id,
    string Name,
    string Type,
    IReadOnlyList<CoordinateDto> Area,
    double MinAltitudeMeters,
    double MaxAltitudeMeters,
    DateTime? ActiveFrom,
    DateTime? ActiveUntil,
    DateTime CreatedAt
);