namespace FlightControl.Dto.Airspace;

public record ZonesAtPointQuery(
    double Latitude,
    double Longitude,
    double AltitudeMeters,
    DateTime? At
);