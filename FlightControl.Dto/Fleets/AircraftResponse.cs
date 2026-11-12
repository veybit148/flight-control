namespace FlightControl.Dto.Fleets;

public record AircraftResponse(
    Guid Id,
    string RegistrationNumber,
    string Category,
    string Status,
    double MaxAltitudeMeters,
    DateTime NextMaintenanceDate,
    Guid OwnerId,
    string OwnerName,
    Guid? FleetId,
    DateTime CreatedAt
);