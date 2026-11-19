using FlightControl.Dal.Enums;
using FlightControl.Domain.Rules;
using FluentAssertions;

namespace FlightControl.Tests.Rules;

public class AirspaceRuleTests
{
    private const string PassedMessage = "The area does not enter any restricted airspace.";

    private readonly AirspaceRule _rule = new();

    [Fact]
    public void Check_NoIntersectingZones_Passes()
    {
        var context = TestData.Context();

        var result = _rule.Check(context);

        result.RuleName.Should().Be("Airspace");
        result.Outcome.Should().Be(CheckOutcome.Passed);
        result.Description.Should().Be(PassedMessage);
    }

    [Theory]
    [InlineData(1000)] // ceiling above the flight
    [InlineData(50)]   // ceiling below the flight: free zones have no upper limit to respect
    public void Check_FreeZone_Passes(double zoneMaxAltitude)
    {
        var context = TestData.Context(
            intersectingZones: [TestData.Zone(type: ZoneType.Free, maxAltitudeMeters: zoneMaxAltitude)]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Passed);
        result.Description.Should().Be(PassedMessage);
    }

    [Fact]
    public void Check_RestrictedZone_Fails()
    {
        var context = TestData.Context(
            intersectingZones: [TestData.Zone(type: ZoneType.Restricted, name: "Airport CTR")]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The area enters the restricted zone 'Airport CTR'.");
    }

    [Theory]
    [InlineData(PermitRequestType.Individual)]
    [InlineData(PermitRequestType.Fleet)]
    public void Check_MilitaryZoneForNonMilitaryRequest_Fails(PermitRequestType requestType)
    {
        var context = TestData.Context(
            permit: TestData.Permit(type: requestType),
            intersectingZones: [TestData.Zone(type: ZoneType.Military, name: "Base")]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The area enters the military zone 'Base'.");
    }

    [Fact]
    public void Check_MilitaryZoneForMilitaryRequest_Passes()
    {
        var context = TestData.Context(
            permit: TestData.Permit(type: PermitRequestType.Military),
            intersectingZones: [TestData.Zone(type: ZoneType.Military, name: "Base")]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Passed);
        result.Description.Should().Be(PassedMessage);
    }

    [Fact]
    public void Check_ControlledZone_Warns()
    {
        var context = TestData.Context(
            intersectingZones: [TestData.Zone(type: ZoneType.Controlled, name: "TMA")]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Warning);
        result.Description.Should().Be("The area enters the controlled zone 'TMA'.");
    }

    [Theory]
    [InlineData(ZoneType.Restricted, TestData.FlightMaxAltitudeMeters)]       // floor exactly at the flight's ceiling
    [InlineData(ZoneType.Restricted, TestData.FlightMaxAltitudeMeters + 100)]
    [InlineData(ZoneType.Military, TestData.FlightMaxAltitudeMeters)]
    [InlineData(ZoneType.Controlled, TestData.FlightMaxAltitudeMeters)]
    public void Check_ZoneFloorAtOrAboveFlightCeiling_IsIgnored(ZoneType zoneType, double zoneMinAltitude)
    {
        var context = TestData.Context(
            intersectingZones:
            [
                TestData.Zone(type: zoneType, minAltitudeMeters: zoneMinAltitude, maxAltitudeMeters: 2000),
            ]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Passed);
        result.Description.Should().Be(PassedMessage);
    }

    [Fact]
    public void Check_FlightAboveZoneUpperLimit_Warns()
    {
        // A military request in a military zone has no other finding, so only the upper-limit warning remains.
        var context = TestData.Context(
            permit: TestData.Permit(type: PermitRequestType.Military, maxAltitudeMeters: 120),
            intersectingZones: [TestData.Zone(type: ZoneType.Military, name: "Base", maxAltitudeMeters: 100)]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Warning);
        result.Description.Should().Be("The flight goes above the upper limit of 'Base'.");
    }

    [Fact]
    public void Check_FlightAtZoneUpperLimit_DoesNotWarn()
    {
        var context = TestData.Context(
            permit: TestData.Permit(type: PermitRequestType.Military, maxAltitudeMeters: 120),
            intersectingZones: [TestData.Zone(type: ZoneType.Military, maxAltitudeMeters: 120)]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Passed);
    }
}
