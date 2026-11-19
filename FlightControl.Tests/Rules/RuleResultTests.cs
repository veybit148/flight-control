using FlightControl.Dal.Enums;
using FlightControl.Domain.Rules;
using FluentAssertions;

namespace FlightControl.Tests.Rules;

public class RuleResultTests
{
    [Fact]
    public void From_FailuresAndWarnings_FailedWithEveryFinding()
    {
        var result = RuleResult.From("Test", ["F1.", "F2."], ["W1."], "OK.");

        result.RuleName.Should().Be("Test");
        result.Outcome.Should().Be(CheckOutcome.Failed);
        result.Description.Should().Be("F1. F2. W1.");
    }

    [Fact]
    public void From_WarningsWithoutFailures_Warning()
    {
        var result = RuleResult.From("Test", [], ["W1.", "W2."], "OK.");

        result.Outcome.Should().Be(CheckOutcome.Warning);
        result.Description.Should().Be("W1. W2.");
    }

    [Fact]
    public void From_NoFindings_PassedWithPassedMessage()
    {
        var result = RuleResult.From("Test", [], [], "OK.");

        result.Outcome.Should().Be(CheckOutcome.Passed);
        result.Description.Should().Be("OK.");
    }
}
