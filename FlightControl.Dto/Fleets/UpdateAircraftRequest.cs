namespace FlightControl.Dto.Fleets;

public record UpdateAircraftRequest(
    string RegistrationNumber,
    double MaxAltitudeMeters,
    DateTime NextMaintenanceDate,
    Guid? FleetId
);