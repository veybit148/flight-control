using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlightControl.Dal.Enums;
using FlightControl.Domain.Security;

namespace FlightControl.Api.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            string? value = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(value, out Guid result) ? result : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            string? value = User?.FindFirst("role")?.Value;
            return Enum.TryParse(value, out UserRole result) ? result : null;
        }
    }
}
