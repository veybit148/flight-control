namespace FlightControl.Dto.Permits;

public record PermitQuery(
    string? Status,
    string? Type,
    DateTime? From,
    DateTime? To,
    string? Sort,
    int Page = 1,
    int PageSize = 20
);