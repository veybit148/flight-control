using FlightControl.Dal.Enums;

namespace FlightControl.Repository.Filters;

public record PermitFilter(
    Guid? RequesterId,
    PermitStatus? Status,
    PermitRequestType? Type,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize,
    bool MilitaryFirst
);