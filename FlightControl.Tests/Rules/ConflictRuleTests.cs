using FlightControl.Dal.Enums;
using FlightControl.Domain.Rules;
using FluentAssertions;

namespace FlightControl.Tests.Rules;

public class ConflictRuleTests
{
    private static readonly Guid OtherAircraftId = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
    private static readonly Guid OtherPilotId = Guid.Parse("00000000-0000-0000-0000-0000000000b1");

    private readonly ConflictRule _rule = new();

    [Fact]
    public void Check_NoOverlappingPermits_Passes()
    {
        var context = TestData.Context();

        var result = _rule.Check(context);

        result.RuleName.Should().Be("Conflict");
        result.Outcome.Should().Be(CheckOutcome.Passed);
        result.Description.Should().Be("No conflicting approved flights.");
    }

    [Fact]
    public void Check_SameAircraftHasOverlappingApprovedPermit_Fails()
    {
        var other = TestData.Permit(aircraftId: TestData.AircraftId, pilotId: OtherPilotId);
        var context = TestData.Context(timeOverlappingApproved: [other]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The aircraft already has an approved permit for overlapping times.");
    }

    [Fact]
    public void Check_SamePilotHasOverlappingApprovedPermit_Fails()
    {
        var other = TestData.Permit(aircraftId: OtherAircraftId, pilotId: TestData.PilotId);
        var context = TestData.Context(timeOverlappingApproved: [other]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The pilot already has an approved permit for overlapping times.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void Check_OtherFlightsUseSameArea_Warns(int otherFlights)
    {
        var others = Enumerable.Range(0, otherFlights)
            .Select(_ => TestData.Permit(aircraftId: OtherAircraftId, pilotId: OtherPilotId))
            .ToList();
        var context = TestData.Context(areaAndTimeOverlappingApproved: others);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Warning);
        result.Description.Should()
            .Be($"{otherFlights} other approved flight(s) use the same area at overlapping times.");
    }

    [Fact]
    public void Check_SameAreaFlightOfSameAircraft_IsNotCountedAsAreaConflict()
    {
        // Already reported as an aircraft conflict; it must not also count as "another flight" in the area.
        var other = TestData.Permit(aircraftId: TestData.AircraftId, pilotId: OtherPilotId);
        var context = TestData.Context(
            timeOverlappingApproved: [other],
            areaAndTimeOverlappingApproved: [other]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The aircraft already has an approved permit for overlapping times.");
    }
}
