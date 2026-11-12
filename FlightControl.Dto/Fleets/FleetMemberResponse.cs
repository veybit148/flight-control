namespace FlightControl.Dto.Fleets;

public record FleetMemberResponse(
    Guid PilotId,
    string FullName,
    string Email,
    DateTime JoinedAt
);