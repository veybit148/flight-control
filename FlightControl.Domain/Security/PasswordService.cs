using FlightControl.Dal.Entities;
using Microsoft.AspNetCore.Identity;

namespace FlightControl.Domain.Security;

public class PasswordService : IPasswordService
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordService(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    // The default PasswordHasher ignores the user argument. 
    // We pass null because a User cannot be created before its password hash exists

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword, password);
        if(result == PasswordVerificationResult.Failed)
        {
            return false;
        }
        else return true;
    }

}