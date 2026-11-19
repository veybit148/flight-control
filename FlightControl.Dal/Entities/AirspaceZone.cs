using FlightControl.Dal.Enums;
using NetTopologySuite.Geometries;

namespace FlightControl.Dal.Entities;

public class AirspaceZone : BaseEntity
{
    public required string Name { get; set; }
    public required ZoneType Type { get; set; }
    public required Polygon Area { get; set; }
    public required double MinAltitudeMeters { get; set; }
    public required double MaxAltitudeMeters { get; set; }
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveUntil { get; set; }
}