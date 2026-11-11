namespace FlightControl.Dal.Entities;

public class FleetMembership : BaseEntity
{
    public required Guid PilotId {get; set;}
    public required Guid FleetId {get; set;}

}