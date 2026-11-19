using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace FlightControl.Dal.Spatial;

public static class Geo
{
    public const int Wgs84 = 4326;

    private static readonly GeometryFactory Factory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: Wgs84);

    public static Point CreatePoint(double latitude, double longitude) =>
        Factory.CreatePoint(new Coordinate(longitude, latitude));

    // Builds a polygon from (latitude, longitude) pairs and closes the ring if needed.
    public static Polygon CreatePolygon(IEnumerable<(double Latitude, double Longitude)> points)
    {
        List<Coordinate> ring = points
            .Select(p => new Coordinate(p.Longitude, p.Latitude))
            .ToList();

        if (ring.Count > 0 && !ring[0].Equals2D(ring[^1]))
        {
            ring.Add(ring[0].Copy());
        }

        return Factory.CreatePolygon(ring.ToArray());
    }
}