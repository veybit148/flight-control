namespace FlightControl.Dto.Fleets;

public record AircraftQuery(
    Guid? FleetId,
    string? Status,
    string? Category
);