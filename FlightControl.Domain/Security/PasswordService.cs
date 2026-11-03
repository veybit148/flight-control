using FlightControl.Dal.Entities;
using Microsoft.AspNetCore.Identity;

namespace FlightControl.Domain.Security;

public class PasswordService : IPasswordService
{
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly string _randomHash;

    public PasswordService(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;

        // Verifies against a dummy hash so that unknown emails take as long as wrong passwords.
        _randomHash = this.HashPassword("S0sTr0ng9_ssWRd"); 
    }

    // The default PasswordHasher ignores the user argument. 
    // We pass null because a User cannot be created before its password hash exists

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string? hashedPassword, string password)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            _ =
            _passwordHasher.VerifyHashedPassword(null!, _randomHash, password);    
            return false;   
        }
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword, password);
        if(result == PasswordVerificationResult.Failed)
        {
            return false;
        }
        else return true;
    }
}