using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Security;
using FlightControl.Domain.Common;
using FlightControl.Dto.Users;
using FlightControl.Repository.Interfaces;
using FlightControl.Validation.Core;
using FlightControl.Dto.Auth;
using FlightControl.Dto.Enums;
using Microsoft.Extensions.Logging;

namespace FlightControl.Domain.Operations.Auth;

public class AuthOperation : IAuthOperation
{
    // Every invalid refresh token gets the same message, so the response reveals nothing about why.
    private const string InvalidRefreshTokenMessage = "Invalid Refresh Token.";

    private readonly AbstractValidator<RegisterRequest> _registerValidator;
    private readonly AbstractValidator<LoginRequest> _loginValidator;
    private readonly AbstractValidator<ChangePasswordRequest> _changePasswordValidator;

    private readonly IUserRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AuthOperation> _logger;

    public AuthOperation(
        AbstractValidator<RegisterRequest> registerValidator,
        AbstractValidator<LoginRequest> loginValidator,
        AbstractValidator<ChangePasswordRequest> changePasswordValidator,
        IUserRepository repository,
        IPasswordService passwordService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<AuthOperation> logger)
    {
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
        _repository = repository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        _registerValidator.ThrowIfInvalid(request);

        RegisterRequest normalizedRequest = 
        request with {Email = EmailNormalizer.Normalize(request.Email)};

        if(await _repository.ExistsByEmailAsync(normalizedRequest.Email))
        {
            throw new ConflictException("This email is already in use.");
        }

        string hashedPassword = _passwordService.HashPassword(request.Password);

        // Military accounts need authority approval before they can log in.
        UserStatus status = normalizedRequest.Role == RegistrationRole.Military
            ? UserStatus.PendingApproval
            : UserStatus.Active;

        User user = normalizedRequest.ToEntity(hashedPassword, status);
    
        _repository.Add(user);

        await _unitOfWork.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        _loginValidator.ThrowIfInvalid(request);
        
        LoginRequest normalizedRequest = 
        request with {Email = EmailNormalizer.Normalize(request.Email)};
        
        User? user = await _repository.GetByEmailAsync(normalizedRequest.Email);
        
        if(user == null)
        {
            _passwordService.VerifyPassword(null, request.Password);
            throw new InvalidCredentialsException("Invalid Email or Password.");
        }

        if(!_passwordService.VerifyPassword(user.PasswordHash, request.Password))
        {
            throw new InvalidCredentialsException("Invalid Email or Password.");
        }

        // Checked only after the password, so the status is never revealed to someone without it.
        EnsureCanLogIn(user);

        AccessToken accessToken = _tokenService.GenerateToken(user.Id, user.Role);


        DateTime now = DateTime.UtcNow;
        GeneratedRefreshToken generatedRefreshToken = _refreshTokenService.CreateRefreshToken();
        (DateTime expires, DateTime sessionExpires) refreshTokenExpires = 
            _refreshTokenService.CalculateExpiries(now);

        RefreshToken refreshToken = new RefreshToken
        {
            TokenHash = generatedRefreshToken.TokenHash,
            UserId = user.Id,
            ExpiresAt = refreshTokenExpires.expires,
            SessionExpiresAt = refreshTokenExpires.sessionExpires,
            RevokedAt = null
        };
    
        _refreshTokenRepository.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync();
        
        TokenResponse tokenResponse = new TokenResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            generatedRefreshToken.Token,
            refreshTokenExpires.expires
        );

        LoginResponse response = user.ToResponse().ToLoginResponse(tokenResponse);   
        return response;
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest request)
    {
        DateTime now = DateTime.UtcNow;
        string hashedRefreshToken = _refreshTokenService.Hash(request.RefreshToken);
        RefreshToken oldToken = await _refreshTokenRepository.GetByHashAsync(hashedRefreshToken) ??
        throw new UnauthorizedException(InvalidRefreshTokenMessage);

        if(oldToken.RevokedAt != null)
        {
            IReadOnlyList<RefreshToken> list =
            await _refreshTokenRepository.GetActiveByUserIdAsync(oldToken.UserId);

            foreach(RefreshToken t in list)
            {
                t.RevokedAt = now;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning(
                "Refresh token reuse detected for user {UserId}; revoked {Count} active session(s).",
                oldToken.UserId, list.Count);

            throw new UnauthorizedException(InvalidRefreshTokenMessage);
        }

        if(now >= oldToken.ExpiresAt)
        {
            throw new UnauthorizedException(InvalidRefreshTokenMessage);
        }

        User? user = await _repository.GetByIdAsync(oldToken.UserId);

        if(user == null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedException(InvalidRefreshTokenMessage);
        }

        GeneratedRefreshToken newRefreshToken = _refreshTokenService.CreateRefreshToken();

        (DateTime expiresAt, DateTime sessionExpiresAt) expires =
            _refreshTokenService.CalculateExpiries(now, oldToken.SessionExpiresAt);

        RefreshToken refreshToken = new RefreshToken
        {
            TokenHash = newRefreshToken.TokenHash,
            UserId = oldToken.UserId,
            ExpiresAt = expires.expiresAt,
            SessionExpiresAt = expires.sessionExpiresAt,
        };
        _refreshTokenRepository.Add(refreshToken);

        AccessToken accessToken = _tokenService.GenerateToken(user.Id, user.Role);    

        TokenResponse tokenResponse = new(
            AccessToken: accessToken.Token,
            AccessTokenExpiresAt: accessToken.ExpiresAt,
            RefreshToken: newRefreshToken.Token,
            RefreshTokenExpiresAt: expires.expiresAt
        );

        oldToken.RevokedAt = now;

        await _unitOfWork.SaveChangesAsync();
        return tokenResponse;
    }

    public async Task LogoutAsync(RefreshRequest request)
    {
        // The caller always gets the same response, so this never reveals whether a token exists.
        string hashedRefreshToken = _refreshTokenService.Hash(request.RefreshToken);
        RefreshToken? token = await _refreshTokenRepository.GetByHashAsync(hashedRefreshToken);

        DateTime now = DateTime.UtcNow;

        if(token == null || token.RevokedAt != null || now >= token.ExpiresAt)
        {
            return;
        }

        token.RevokedAt = now;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        _changePasswordValidator.ThrowIfInvalid(request);

        User user = await _repository.GetByIdAsync(CurrentUserId) ??
            throw new UnauthorizedException("User account no longer exists.");

        if(!_passwordService.VerifyPassword(user.PasswordHash, request.CurrentPassword))
        {
            ValidationResult result = new();
            result.AddError(nameof(request.CurrentPassword), "Current password is incorrect.");
            throw new ValidationException(result);
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);

        // Every session ends with the old password. Access tokens already issued stay valid until they expire.
        DateTime now = DateTime.UtcNow;
        IReadOnlyList<RefreshToken> activeTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(user.Id);

        foreach(RefreshToken token in activeTokens)
        {
            token.RevokedAt = now;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    // Helpers

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new InvalidOperationException("AuthOperation requires an authenticated user here.");

    private static void EnsureCanLogIn(User user)
    {
        switch (user.Status)
        {
            case UserStatus.Active:
                return;
            case UserStatus.PendingApproval:
                throw new ForbiddenException("Your account is pending approval by the authority.");
            case UserStatus.Suspended:
                throw new ForbiddenException("Your account is suspended.");
            default:
                throw new InvalidOperationException($"Unknown user status {user.Status}.");
        }
    }
}