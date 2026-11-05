using FlightControl.Dto.Users;

namespace FlightControl.Domain.Operations.Users;

public interface IUserOperation
{
    Task<UserResponse> GetMeAsync();
}