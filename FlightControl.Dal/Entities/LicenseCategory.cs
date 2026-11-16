using FlightControl.Dal.Enums;

namespace FlightControl.Dal.Entities;

public class LicenseCategory : BaseEntity
{
    public required Guid LicenseId { get; set; }    
    public required AircraftCategory Category { get; set; }
}