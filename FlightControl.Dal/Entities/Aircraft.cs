using FlightControl.Dal.Enums;

namespace FlightControl.Dal.Entities;

public class Aircraft : BaseEntity
{
    public required Guid OwnerId {get; set;}
    public Guid? FleetId {get; set;}
    public required string RegistrationNumber {get; set;}
    public required double MaxAltitudeMeters {get; set;}
    public required AircraftCategory Category {get; set;}
    public AircraftStatus Status {get; set;} = AircraftStatus.Active;
    public required DateTime NextMaintenanceDate {get; set;}
}