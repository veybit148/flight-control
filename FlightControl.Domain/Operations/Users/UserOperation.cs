using FlightControl.Dal.Entities;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Security;
using FlightControl.Dto.Users;
using FlightControl.Repository.Interfaces;

namespace FlightControl.Domain.Operations.Users;

public class UserOperation: IUserOperation
{
    private readonly IUserRepository _repository;
    private readonly ICurrentUser _currentUser;

    public UserOperation(
        IUserRepository repository,
        ICurrentUser currentUser
    )
    {
        _repository = repository;
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
}