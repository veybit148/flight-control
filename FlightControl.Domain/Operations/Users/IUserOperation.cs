using FlightControl.Dto.Users;

namespace FlightControl.Domain.Operations.Users;

public interface IUserOperation
{
    Task<UserResponse> GetMeAsync();
    Task<IReadOnlyList<UserResponse>> ListPendingAsync();
    Task<UserResponse> ApproveAsync(Guid id);
    Task<UserResponse> SuspendAsync(Guid id);
    Task<UserResponse> ReactivateAsync(Guid id);
}
