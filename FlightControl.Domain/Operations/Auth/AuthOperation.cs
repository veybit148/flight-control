using FlightControl.Dal.Entities;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Security;
using FlightControl.Domain.Common;
using FlightControl.Dto.Users;
using FlightControl.Repository.Interfaces;
using FlightControl.Validation.Core;
using FlightControl.Dto.Auth;

namespace FlightControl.Domain.Operations.Auth;

public class AuthOperation : IAuthOperation
{
    private readonly AbstractValidator<RegisterRequest> _registerValidator;
    private readonly AbstractValidator<LoginRequest> _loginValidator;

    private readonly IUserRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthOperation(
        AbstractValidator<RegisterRequest> registerValidator,
        AbstractValidator<LoginRequest> loginValidator,
        IUserRepository repository,
        IPasswordService passwordService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _repository = repository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
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

        User user = normalizedRequest.ToEntity(hashedPassword);
    
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
        throw new UnauthorizedException("Invalid Refresh Token.");

        if(oldToken.RevokedAt != null)
        {
            IReadOnlyList<RefreshToken> list = 
            await _refreshTokenRepository.GetActiveByUserIdAsync(oldToken.UserId);

            foreach(RefreshToken t in list)
            {
                t.RevokedAt = now;
            }

            await _unitOfWork.SaveChangesAsync();
            throw new UnauthorizedException("Invalid Refresh Token.");
        }

        if(now >= oldToken.ExpiresAt)
        {
            throw new UnauthorizedException("Invalid Refresh Token.");
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

        User user = await _repository.GetByIdAsync(oldToken.UserId) ??
            throw new UnauthorizedException("Invalid Refresh Token.");

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
}