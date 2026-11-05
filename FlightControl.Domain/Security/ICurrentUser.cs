using FlightControl.Dal.Enums;

namespace FlightControl.Domain.Security;

public interface ICurrentUser
{
    // Implemented in the Api project by CurrentUser, 
    // which reads the claims of the current HTTP request
    Guid? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}   