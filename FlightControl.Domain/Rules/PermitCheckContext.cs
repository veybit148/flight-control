using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;

namespace FlightControl.Domain.Rules;

public record PermitCheckContext(
    FlightPermitRequest Permit,
    Aircraft Aircraft,
    PilotLicense License,
    IReadOnlyList<AircraftCategory> LicenseCategories,
    IReadOnlyList<AirspaceZone> IntersectingZones,
    IReadOnlyList<FlightPermitRequest> TimeOverlappingApproved,
    IReadOnlyList<FlightPermitRequest> AreaAndTimeOverlappingApproved
);