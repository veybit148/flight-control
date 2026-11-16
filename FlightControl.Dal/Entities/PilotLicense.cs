using FlightControl.Dal.Enums;

namespace FlightControl.Dal.Entities;

public class PilotLicense : BaseEntity
{
    public required Guid PilotId { get; set; }
    public required string LicenseNumber { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public LicenseStatus Status { get; set; } = LicenseStatus.Valid;
    public string? SuspensionReason { get; set; }
}