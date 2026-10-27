using FlightControl.Dto.Users;

namespace FlightControl.Domain.Operations.Auth;

public interface IAuthOperation
{
    public Task<UserResponse> RegisterAsync(RegisterRequest request);
}