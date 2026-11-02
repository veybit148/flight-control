namespace FlightControl.Domain.Security;

public record AccessToken(string Token, DateTime EndTime);