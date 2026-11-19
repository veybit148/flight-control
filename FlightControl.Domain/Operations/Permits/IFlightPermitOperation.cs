using FlightControl.Dto.Common;
using FlightControl.Dto.Permits;

namespace FlightControl.Domain.Operations.Permits;

public interface IFlightPermitOperation
{
    Task<PermitResponse> CreateAsync(CreatePermitRequest request);
    Task<PagedResponse<PermitResponse>> ListAsync(PermitQuery query);
    Task<PermitResponse> GetByIdAsync(Guid id);
    Task<PermitResponse> CancelAsync(Guid id);
    Task<PermitResponse> ApproveAsync(Guid id, ApprovePermitRequest request);
    Task<PermitResponse> RejectAsync(Guid id, RejectPermitRequest request);
}