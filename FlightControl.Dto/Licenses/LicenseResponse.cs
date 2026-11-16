namespace FlightControl.Dto.Licenses;

public record LicenseResponse(
    Guid Id,
    Guid PilotId,
    string PilotName,
    string LicenseNumber,
    DateTime ExpiresAt,
    string Status,
    string? SuspensionReason,
    IReadOnlyList<string> Categories,
    DateTime CreatedAt
);