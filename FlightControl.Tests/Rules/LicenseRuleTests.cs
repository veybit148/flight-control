using FlightControl.Dal.Enums;
using FlightControl.Domain.Rules;
using FluentAssertions;

namespace FlightControl.Tests.Rules;

public class LicenseRuleTests
{
    private readonly LicenseRule _rule = new();

    [Fact]
    public void Check_ValidLicense_Passes()
    {
        var context = TestData.Context();

        var result = _rule.Check(context);

        result.RuleName.Should().Be("License");
        result.Outcome.Should().Be(CheckOutcome.Passed);
        result.Description.Should().Be("The license is valid for this flight.");
    }

    [Fact]
    public void Check_SuspendedLicense_Fails()
    {
        var context = TestData.Context(license: TestData.License(status: LicenseStatus.Suspended));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The license is suspended.");
    }

    [Theory]
    [InlineData(-1)]   // expires before the flight starts
    [InlineData(30)]   // expires during the flight
    [InlineData(59)]   // expires one minute before the flight ends
    public void Check_LicenseExpiresBeforeFlightEnds_Fails(int minutesAfterStart)
    {
        var expiresAt = TestData.FlightStart.AddMinutes(minutesAfterStart);
        var context = TestData.Context(license: TestData.License(expiresAt: expiresAt));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The license expires before the flight ends.");
    }

    [Fact]
    public void Check_AircraftCategoryNotCovered_Fails()
    {
        var context = TestData.Context(
            aircraft: TestData.Aircraft(category: AircraftCategory.Helicopter),
            licenseCategories: [AircraftCategory.UAV, AircraftCategory.Glider]);

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("The license does not cover the Helicopter category.");
    }

    [Theory]
    [InlineData(0)]    // expires exactly when the flight ends
    [InlineData(1)]
    [InlineData(29)]
    public void Check_LicenseExpiresWithin30DaysAfterFlight_Warns(int daysAfterEnd)
    {
        var expiresAt = TestData.FlightEnd.AddDays(daysAfterEnd);
        var context = TestData.Context(license: TestData.License(expiresAt: expiresAt));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Warning);
        result.Description.Should().Be("The license expires within 30 days after the flight.");
    }

    [Fact]
    public void Check_LicenseExpiresExactly30DaysAfterFlight_Passes()
    {
        var context = TestData.Context(license: TestData.License(expiresAt: TestData.FlightEnd.AddDays(30)));

        var result = _rule.Check(context);

        result.Outcome.Should().Be(CheckOutcome.Passed);
    }
}
