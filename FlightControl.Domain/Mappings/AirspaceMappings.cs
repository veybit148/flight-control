using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Dal.Spatial;
using FlightControl.Dto.Airspace;
using NetTopologySuite.Geometries;

namespace FlightControl.Domain.Mappings;

public static class AirspaceMappings
{
    public static AirspaceZone ToEntity(this AirspaceZoneRequest request) => new()
    {
        Name = request.Name.Trim(),
        Type = request.Type.ToEnum<ZoneType>(),
        Area = Geo.CreatePolygon(request.Area.Select(p => (p.Latitude, p.Longitude))),
        MinAltitudeMeters = request.MinAltitudeMeters,
        MaxAltitudeMeters = request.MaxAltitudeMeters,
        ActiveFrom = request.ActiveFrom,
        ActiveUntil = request.ActiveUntil
    };

    public static void ApplyTo(this AirspaceZoneRequest request, AirspaceZone zone)
    {
        zone.Name = request.Name.Trim();
        zone.Type = request.Type.ToEnum<ZoneType>();
        zone.Area = Geo.CreatePolygon(request.Area.Select(p => (p.Latitude, p.Longitude)));
        zone.MinAltitudeMeters = request.MinAltitudeMeters;
        zone.MaxAltitudeMeters = request.MaxAltitudeMeters;
        zone.ActiveFrom = request.ActiveFrom;
        zone.ActiveUntil = request.ActiveUntil;
    }

    public static AirspaceZoneResponse ToResponse(this AirspaceZone zone) => new(
        zone.Id,
        zone.Name,
        zone.Type.ToString(),
        zone.Area.ToCoordinateDtos(),
        zone.MinAltitudeMeters,
        zone.MaxAltitudeMeters,
        zone.ActiveFrom,
        zone.ActiveUntil,
        zone.CreatedAt
    );

    public static IReadOnlyList<CoordinateDto> ToCoordinateDtos(this Polygon polygon) =>
    polygon.ExteriorRing.Coordinates
        .Select(c => new CoordinateDto(c.Y, c.X))
        .ToList();
}