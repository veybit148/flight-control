using System.Text;
using FlightControl.Dal.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FlightControl.Domain.Security;

public class TokenService: ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly SigningCredentials _credentials;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public AccessToken GenerateToken(Guid userId, UserRole role)
    {
        DateTime tokenEnd = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        
        SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor();
        descriptor.Issuer = _jwtOptions.Issuer;
        descriptor.Audience = _jwtOptions.Audience;
        descriptor.Claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.ToString(),
            ["role"] = role.ToString()            
        };
        descriptor.SigningCredentials = _credentials;
        descriptor.Expires = tokenEnd;

        JsonWebTokenHandler jsonWebTokenHandler = new JsonWebTokenHandler();
        string token = jsonWebTokenHandler.CreateToken(descriptor);

        return new AccessToken(token, tokenEnd);
    }
}