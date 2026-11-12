namespace FlightControl.Dto.Fleets;

public record CreateAircraftRequest(
    string RegistrationNumber,
    string Category,
    double MaxAltitudeMeters,
    DateTime NextMaintenanceDate,
    Guid? FleetId
);