namespace FlightControl.Dto.Users;

public record LoginRequest(
    string Email,
    string Password
);