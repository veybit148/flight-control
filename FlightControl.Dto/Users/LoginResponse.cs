namespace FlightControl.Dto.Users;

public record LoginResponse(
    UserResponse User,
    string Token,
    DateTime ExpiresAt
);