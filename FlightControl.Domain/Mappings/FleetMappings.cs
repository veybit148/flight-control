using FlightControl.Dal.Entities;
using FlightControl.Dto.Fleets;

namespace FlightControl.Domain.Mappings;

public static class FleetMappings
{
    public static Fleet ToEntity(this CreateFleetRequest request, Guid ownerId) => new()
    {
        Name = request.Name,
        OwnerId = ownerId
    };

    public static FleetResponse ToResponse(this Fleet fleet) => new(
        fleet.Id,
        fleet.Name,
        fleet.OwnerId,
        fleet.CreatedAt
    );

    public static FleetMemberResponse ToMemberResponse(this FleetMembership membership, User pilot) => new(
        pilot.Id,
        pilot.FullName,
        pilot.Email,
        membership.CreatedAt
    );
}