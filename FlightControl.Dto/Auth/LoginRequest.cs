namespace FlightControl.Dto.Auth;

public record LoginRequest(
    string Email,
    string Password
    );