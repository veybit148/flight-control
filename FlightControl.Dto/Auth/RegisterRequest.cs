using FlightControl.Dto.Enums;

namespace FlightControl.Dto.Auth;

public record RegisterRequest(
    string Email, 
    string Password, 
    string FullName,
    RegistrationRole Role    
    );