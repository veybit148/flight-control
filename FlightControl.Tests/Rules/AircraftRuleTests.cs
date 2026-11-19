using FlightControl.Dal.Enums;
using FlightControl.Domain.Rules;
using FluentAssertions;

namespace FlightControl.Tests.Rules;

public class AircraftRuleTests
{
    private readonly AircraftRule _rule = new();

    [Fact]
    public void Check_FitAircraft_Passes()
    {
        var context = TestData.Context();

        var result = _rule.Check(context);

        result.RuleName.Should().Be("Aircraft");
        result.Outcome.Should().Be(CheckOutcome.Passed);
        result.Description.Should().Be("The aircraft is fit for this flight.");
    }

    [Fact]
    public void Check_GroundedAircraft_Fails()
    {
        var context = TestData.Context(aircraft: TestData.Aircraft(status: AircraftStatus.Grounded));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The aircraft is grounded.");
    }

    [Fact]
    public void Check_AircraftInMaintenance_Fails()
    {
        var context = TestData.Context(aircraft: TestData.Aircraft(status: AircraftStatus.Maintenance));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The aircraft is in maintenance.");
    }

    [Theory]
    [InlineData(-60)]  // maintenance was due before the flight
    [InlineData(0)]    // due when the flight starts
    [InlineData(59)]   // due one minute before the flight ends
    public void Check_MaintenanceDueBeforeFlightEnds_Fails(int minutesAfterStart)
    {
        var dueAt = TestData.FlightStart.AddMinutes(minutesAfterStart);
        var context = TestData.Context(aircraft: TestData.Aircraft(nextMaintenanceDate: dueAt));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("Maintenance is due before the flight ends.");
    }

    [Theory]
    [InlineData(TestData.AircraftMaxAltitudeMeters + 0.1)]
    [InlineData(TestData.AircraftMaxAltitudeMeters * 2)]
    public void Check_AltitudeAboveAircraftLimit_Fails(double requestedAltitude)
    {
        var context = TestData.Context(permit: TestData.Permit(maxAltitudeMeters: requestedAltitude));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The requested altitude is above the aircraft's limit of 500 m.");
    }

    [Fact]
    public void Check_AltitudeEqualToAircraftLimit_Passes()
    {
        var context = TestData.Context(
            permit: TestData.Permit(maxAltitudeMeters: TestData.AircraftMaxAltitudeMeters));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Passed);
    }

    [Theory]
    [InlineData(0)]    // due exactly when the flight ends
    [InlineData(1)]
    [InlineData(6)]
    public void Check_MaintenanceDueWithin7DaysAfterFlight_Warns(int daysAfterEnd)
    {
        var dueAt = TestData.FlightEnd.AddDays(daysAfterEnd);
        var context = TestData.Context(aircraft: TestData.Aircraft(nextMaintenanceDate: dueAt));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Warning);
        result.Description.Should().Be("Maintenance is due within 7 days after the flight.");
    }

    [Fact]
    public void Check_MaintenanceDueExactly7DaysAfterFlight_Passes()
    {
        var context = TestData.Context(
            aircraft: TestData.Aircraft(nextMaintenanceDate: TestData.FlightEnd.AddDays(7)));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Passed);
    }
}
