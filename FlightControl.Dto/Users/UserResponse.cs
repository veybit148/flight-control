namespace FlightControl.Dto.Users;

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    DateTime CreatedAt,
    string Role
);