using FlightControl.Dal.Spatial;
using FluentAssertions;

namespace FlightControl.Tests.Spatial;

public class GeoTests
{
    [Fact]
    public void CreatePolygon_OpenRing_ClosesRing()
    {
        (double Latitude, double Longitude)[] points =
        [
            (41.00, 29.00),
            (41.00, 29.01),
            (41.01, 29.01),
            (41.01, 29.00),
        ];

        var polygon = Geo.CreatePolygon(points);

        var ring = polygon.ExteriorRing.Coordinates;
        ring.Should().HaveCount(5);
        ring[^1].Equals2D(ring[0]).Should().BeTrue();
        polygon.IsValid.Should().BeTrue();
        polygon.SRID.Should().Be(Geo.Wgs84);
    }

    [Fact]
    public void CreatePolygon_ClosedRing_DoesNotAddPoint()
    {
        (double Latitude, double Longitude)[] points =
        [
            (41.00, 29.00),
            (41.00, 29.01),
            (41.01, 29.01),
            (41.01, 29.00),
            (41.00, 29.00),
        ];

        var polygon = Geo.CreatePolygon(points);

        polygon.ExteriorRing.Coordinates.Should().HaveCount(5);
    }

    [Fact]
    public void CreatePolygon_LatitudeLongitudePairs_StoresLongitudeAsX()
    {
        var polygon = Geo.CreatePolygon([(41.0, 29.0), (41.0, 29.01), (41.01, 29.01)]);

        var first = polygon.ExteriorRing.Coordinates[0];
        first.X.Should().Be(29.0);
        first.Y.Should().Be(41.0);
    }

    [Fact]
    public void CreatePoint_LatitudeLongitude_StoresLongitudeAsXAndLatitudeAsY()
    {
        var point = Geo.CreatePoint(latitude: 41.0082, longitude: 28.9784);

        point.X.Should().Be(28.9784);
        point.Y.Should().Be(41.0082);
        point.SRID.Should().Be(Geo.Wgs84);
    }
}
