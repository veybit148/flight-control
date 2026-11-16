namespace FlightControl.Dto.Licenses;

public record CreateLicenseRequest(
    string LicenseNumber,
    DateTime ExpiresAt,
    IReadOnlyList<string> Categories
);