using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlightControl.Domain.Security;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly RefreshTokenOptions _options;

    public RefreshTokenService(IOptions<RefreshTokenOptions> options)
    {
        _options = options.Value;
    }
    
    public GeneratedRefreshToken CreateRefreshToken()
    {
        string token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        string tokenHash = Hash(token);
        return new GeneratedRefreshToken(token, tokenHash);
    }

    public string Hash(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public (DateTime expires, DateTime sessionExpires) 
    CalculateExpiries(DateTime now, DateTime? sessionEnd = null)
    {
        DateTime sessionExpires = sessionEnd ?? now.AddDays(_options.AbsoluteLifetimeDays);
        DateTime slidingExpire = now.AddDays(_options.SlidingLifetimeDays);
        DateTime expires = 
        slidingExpire >= sessionExpires ? 
        sessionExpires : slidingExpire;
        return (expires, sessionExpires);
    }
}