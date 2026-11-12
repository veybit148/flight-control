namespace FlightControl.Dto.Fleets;

public record ChangeAircraftStatusRequest(
    string Status,
    string? Reason
);