using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Security;
using FlightControl.Dto.Users;
using FlightControl.Repository.Interfaces;

namespace FlightControl.Domain.Operations.Users;

public class UserOperation: IUserOperation
{
    private const string UserNotFoundMessage = "User not found.";

    private readonly IUserRepository _repository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UserOperation(
        IUserRepository repository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
    )
    {
        _repository = repository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UserResponse> GetMeAsync()
    {
        Guid userId = _currentUser.UserId ??
            throw new InvalidOperationException($"{nameof(GetMeAsync)} must be called frome an Authorized controler.");

        User user = await _repository.GetByIdAsync(userId) ??
            throw new UnauthorizedException("User account no longer exists.");

        return user.ToResponse();
    }

    public async Task<IReadOnlyList<UserResponse>> ListPendingAsync()
    {
        EnsureAuthority();

        IReadOnlyList<User> users = await _repository.GetByStatusAsync(UserStatus.PendingApproval);

        return users
            .Select(u => u.ToResponse())
            .ToList();
    }

    public async Task<UserResponse> ApproveAsync(Guid id)
    {
        User user = await GetUserForStatusChangeAsync(id, UserStatus.Active, UserStatus.PendingApproval);

        user.Status = UserStatus.Active;
        await _unitOfWork.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<UserResponse> SuspendAsync(Guid id)
    {
        User user = await GetUserForStatusChangeAsync(
            id, UserStatus.Suspended, UserStatus.PendingApproval, UserStatus.Active);

        DateTime now = DateTime.UtcNow;
        IReadOnlyList<RefreshToken> activeTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(user.Id);

        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAt = now;
        }

        user.Status = UserStatus.Suspended;

        // The status and the revoked tokens are written together.
        await _unitOfWork.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<UserResponse> ReactivateAsync(Guid id)
    {
        User user = await GetUserForStatusChangeAsync(id, UserStatus.Active, UserStatus.Suspended);

        user.Status = UserStatus.Active;
        await _unitOfWork.SaveChangesAsync();

        return user.ToResponse();
    }

    // Helpers

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new InvalidOperationException("UserOperation requires an authenticated user.");

    private UserRole CurrentRole => _currentUser.Role
        ?? throw new InvalidOperationException("UserOperation requires an authenticated user.");

    private bool IsAuthority => CurrentRole == UserRole.Authority;

    // The controller already restricts these endpoints to the authority; this keeps the
    // operation safe if it is ever called from somewhere else.
    private void EnsureAuthority()
    {
        if (!IsAuthority)
        {
            throw new ForbiddenException("Only the authority can manage user status.");
        }
    }

    // Returns the user if the current status allows the change to the target status.
    private async Task<User> GetUserForStatusChangeAsync(Guid id, UserStatus to, params UserStatus[] allowedFrom)
    {
        EnsureAuthority();

        if (id == CurrentUserId)
        {
            throw new ForbiddenException("You cannot change your own status.");
        }

        User user = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(UserNotFoundMessage);

        if (!allowedFrom.Contains(user.Status))
        {
            throw new ConflictException($"A user with status {user.Status} cannot be changed to {to}.");
        }

        return user;
    }
}
