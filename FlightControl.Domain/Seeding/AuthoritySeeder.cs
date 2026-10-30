
using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Domain.Common;
using FlightControl.Domain.Security;
using FlightControl.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlightControl.Domain.Seeding;

public class AuthoritySeeder
{
    private readonly IUserRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthoritySeeder> _logger;

    public AuthoritySeeder(
        IUserRepository repository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork,
        ILogger<AuthoritySeeder> logger)
    {
        _repository = repository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> SeedAsync(string email, string password)
    {
        if(string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Authority seed skipped: email or password is missing in configuration.");
            return false;
        }

        string normalizedEmail = EmailNormalizer.Normalize(email);

        if(await _repository.ExistsByRoleAsync(UserRole.Authority))
        {
            _logger.LogInformation("Authority seed skipped: An authority already exists.");
            return true;
        }
        
        if(await _repository.ExistsByEmailAsync(normalizedEmail))
        {
            _logger.LogWarning("Authority seed skipped: {Email} already belongs to a non-authority user.",
                normalizedEmail);
            return false;
        }

        string hashedPassword = _passwordService.HashPassword(password);

        User user = new User
        {
            Email = normalizedEmail,
            FullName = "Authority",
            PasswordHash = hashedPassword,
            Role = UserRole.Authority
        };

        _repository.Add(user);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Authority account {Email} created.", normalizedEmail);
        return true;
    }
}