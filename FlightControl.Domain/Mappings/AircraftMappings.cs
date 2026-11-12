using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Dto.Fleets;

namespace FlightControl.Domain.Mappings;

public static class AircraftMappings
{
    public static Aircraft ToEntity(this CreateAircraftRequest request, Guid ownerId) => new()
    {
        OwnerId = ownerId,
        FleetId = request.FleetId,
        RegistrationNumber = request.RegistrationNumber,
        Category = request.Category.ToEnum<AircraftCategory>(),
        MaxAltitudeMeters = request.MaxAltitudeMeters,
        NextMaintenanceDate = request.NextMaintenanceDate
    };

    public static void ApplyTo(this UpdateAircraftRequest request, Aircraft aircraft)
    {
        aircraft.RegistrationNumber = request.RegistrationNumber;
        aircraft.MaxAltitudeMeters = request.MaxAltitudeMeters;
        aircraft.NextMaintenanceDate = request.NextMaintenanceDate;
        aircraft.FleetId = request.FleetId;
    }

    public static AircraftResponse ToResponse(this Aircraft aircraft, string ownerName) => new(
        aircraft.Id,
        aircraft.RegistrationNumber,
        aircraft.Category.ToString(),
        aircraft.Status.ToString(),
        aircraft.MaxAltitudeMeters,
        aircraft.NextMaintenanceDate,
        aircraft.OwnerId,
        ownerName,
        aircraft.FleetId,
        aircraft.CreatedAt
    );
}