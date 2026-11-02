using FlightControl.Dal.Enums;

namespace FlightControl.Domain.Security;

public interface ITokenService
{
    public AccessToken GenerateToken(Guid userId, UserRole userRole);
}