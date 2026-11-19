namespace FlightControl.Dto.Airspace;

public record AirspaceZoneRequest(
    string Name,
    string Type,
    IReadOnlyList<CoordinateDto> Area,
    double MinAltitudeMeters,
    double MaxAltitudeMeters,
    DateTime? ActiveFrom,
    DateTime? ActiveUntil
);