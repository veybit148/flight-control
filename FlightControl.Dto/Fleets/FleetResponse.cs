namespace FlightControl.Dto.Fleets;

public record FleetResponse(
    Guid Id,
    string Name,
    Guid OwnerId,
    DateTime CreatedAt
);