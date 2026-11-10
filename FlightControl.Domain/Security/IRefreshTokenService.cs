namespace FlightControl.Domain.Security;

public interface IRefreshTokenService
{
    GeneratedRefreshToken CreateRefreshToken();
    string Hash(string random);
    
    (DateTime expires, DateTime sessionExpires)
    CalculateExpiries(DateTime now, DateTime? sessionEnd = null);
}
