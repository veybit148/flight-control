using FlightControl.Dto.Enums;

namespace FlightControl.Dto.Users;

public record RegisterRequest(
    string Email, 
    string Password, 
    string FullName,
    RegistrationRole Role    
    );