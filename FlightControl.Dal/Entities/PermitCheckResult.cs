using FlightControl.Dal.Enums;

namespace FlightControl.Dal.Entities;

public class PermitCheckResult : BaseEntity
{
    public required Guid PermitRequestId { get; init; }
    public required string RuleName { get; init; }
    public required CheckOutcome Outcome { get; init; }
    public required string Description { get; init; }
}