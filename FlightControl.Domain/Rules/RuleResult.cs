using FlightControl.Dal.Enums;

namespace FlightControl.Domain.Rules;

public record RuleResult(string RuleName, CheckOutcome Outcome, string Description)
{
    // The worst finding decides the outcome; every finding goes into the description.
    public static RuleResult From(
        string ruleName,
        IReadOnlyList<string> failures,
        IReadOnlyList<string> warnings,
        string passedMessage)
    {
        if (failures.Count > 0)
        {
            return new(ruleName, CheckOutcome.Failed, string.Join(" ", failures.Concat(warnings)));
        }

        if (warnings.Count > 0)
        {
            return new(ruleName, CheckOutcome.Warning, string.Join(" ", warnings));
        }

        return new(ruleName, CheckOutcome.Passed, passedMessage);
    }
}