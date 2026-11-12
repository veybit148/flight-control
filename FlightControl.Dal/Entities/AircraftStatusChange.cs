using FlightControl.Dal.Enums;

namespace FlightControl.Dal.Entities;

public class AircraftStatusChange : BaseEntity
{
    public required Guid AircraftId { get; set; }
    public required AircraftStatus FromStatus { get; set; }
    public required AircraftStatus ToStatus { get; set; }
    public string? Reason { get; set; }
    public required Guid ChangedById { get; set; }
}