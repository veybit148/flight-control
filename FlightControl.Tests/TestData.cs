using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Dal.Spatial;
using FlightControl.Domain.Rules;
using NetTopologySuite.Geometries;

namespace FlightControl.Tests;

// Builds valid, "everything is fine" objects. Each parameter overrides one field.
// Every date is fixed so that tests never depend on the current time.
public static class TestData
{
    public static readonly Guid RequesterId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid PilotId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid AircraftId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid LicenseId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid OwnerId = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid AuthorityId = Guid.Parse("00000000-0000-0000-0000-000000000006");

    public static readonly DateTime FlightStart = new(2030, 6, 1, 10, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime FlightEnd = new(2030, 6, 1, 11, 0, 0, DateTimeKind.Utc);

    public const double FlightMaxAltitudeMeters = 120;
    public const double AircraftMaxAltitudeMeters = 500;

    // A small square (about 1 km) near Istanbul, with its south-west corner at the given point.
    public static Polygon Square(double latitude = 41.0, double longitude = 29.0, double size = 0.01) =>
        Geo.CreatePolygon(
        [
            (latitude, longitude),
            (latitude, longitude + size),
            (latitude + size, longitude + size),
            (latitude + size, longitude),
        ]);

    public static FlightPermitRequest Permit(
        PermitRequestType type = PermitRequestType.Individual,
        Guid? aircraftId = null,
        Guid? pilotId = null,
        Guid? licenseId = null,
        Polygon? area = null,
        DateTime? plannedStart = null,
        DateTime? plannedEnd = null,
        double maxAltitudeMeters = FlightMaxAltitudeMeters) =>
        new()
        {
            RequesterId = RequesterId,
            Type = type,
            AircraftId = aircraftId ?? AircraftId,
            LicenseId = licenseId ?? LicenseId,
            PilotId = pilotId ?? PilotId,
            Area = area ?? Square(),
            PlannedStart = plannedStart ?? FlightStart,
            PlannedEnd = plannedEnd ?? FlightEnd,
            MaxAltitudeMeters = maxAltitudeMeters,
            Purpose = "Survey flight",
        };

    public static Aircraft Aircraft(
        AircraftStatus status = AircraftStatus.Active,
        AircraftCategory category = AircraftCategory.UAV,
        double maxAltitudeMeters = AircraftMaxAltitudeMeters,
        DateTime? nextMaintenanceDate = null) =>
        new()
        {
            Id = AircraftId,
            OwnerId = OwnerId,
            RegistrationNumber = "TC-UAV01",
            MaxAltitudeMeters = maxAltitudeMeters,
            Category = category,
            Status = status,
            NextMaintenanceDate = nextMaintenanceDate ?? FlightEnd.AddDays(180),
        };

    public static PilotLicense License(
        LicenseStatus status = LicenseStatus.Valid,
        DateTime? expiresAt = null) =>
        new()
        {
            Id = LicenseId,
            PilotId = PilotId,
            LicenseNumber = "LIC-0001",
            ExpiresAt = expiresAt ?? FlightEnd.AddYears(1),
            Status = status,
        };

    public static AirspaceZone Zone(
        ZoneType type = ZoneType.Free,
        string name = "Test Zone",
        Polygon? area = null,
        double minAltitudeMeters = 0,
        double maxAltitudeMeters = 1000) =>
        new()
        {
            Name = name,
            Type = type,
            Area = area ?? Square(),
            MinAltitudeMeters = minAltitudeMeters,
            MaxAltitudeMeters = maxAltitudeMeters,
        };

    // By default the license covers the aircraft's category and nothing intersects or overlaps.
    public static PermitCheckContext Context(
        FlightPermitRequest? permit = null,
        Aircraft? aircraft = null,
        PilotLicense? license = null,
        IReadOnlyList<AircraftCategory>? licenseCategories = null,
        IReadOnlyList<AirspaceZone>? intersectingZones = null,
        IReadOnlyList<FlightPermitRequest>? timeOverlappingApproved = null,
        IReadOnlyList<FlightPermitRequest>? areaAndTimeOverlappingApproved = null)
    {
        aircraft ??= Aircraft();

        return new PermitCheckContext(
            permit ?? Permit(),
            aircraft,
            license ?? License(),
            licenseCategories ?? [aircraft.Category],
            intersectingZones ?? [],
            timeOverlappingApproved ?? [],
            areaAndTimeOverlappingApproved ?? []);
    }
}
