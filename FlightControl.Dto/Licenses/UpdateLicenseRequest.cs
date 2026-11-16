namespace FlightControl.Dto.Licenses;

public record UpdateLicenseRequest(
    DateTime ExpiresAt,
    IReadOnlyList<string> Categories
);