using FlightControl.Dto.Users;

namespace FlightControl.Dto.Auth;

public record LoginResponse(
    UserResponse User,
    TokenResponse Tokens
    );