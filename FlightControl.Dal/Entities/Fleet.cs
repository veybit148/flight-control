namespace FlightControl.Dal.Entities;

public class Fleet : BaseEntity
{
    public required string Name {get; set;}
    public required Guid OwnerId {get; set;}
}