using FlightControl.Dto.Auth;
using FlightControl.Dto.Users;

namespace FlightControl.Domain.Operations.Auth;

public interface IAuthOperation
{
    public Task<UserResponse> RegisterAsync(RegisterRequest request);
    public Task<LoginResponse> LoginAsync(LoginRequest request);
    public Task<TokenResponse> RefreshAsync(RefreshRequest request);
    public Task LogoutAsync(RefreshRequest request);
    public Task ChangePasswordAsync(ChangePasswordRequest request);

}