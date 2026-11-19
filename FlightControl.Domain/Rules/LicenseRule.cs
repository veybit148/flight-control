using FlightControl.Dal.Enums;

namespace FlightControl.Domain.Rules;

public class LicenseRule : IPermitRule
{
    private const int ExpiryWarningDays = 30;

    public RuleResult Check(PermitCheckContext context)
    {
        var permit = context.Permit;
        var license = context.License;
        List<string> failures = [];
        List<string> warnings = [];

        if (license.Status == LicenseStatus.Suspended)
        {
            failures.Add("The license is suspended.");
        }

        if (license.ExpiresAt < permit.PlannedEnd)
        {
            failures.Add("The license expires before the flight ends.");
        }
        else if (license.ExpiresAt < permit.PlannedEnd.AddDays(ExpiryWarningDays))
        {
            warnings.Add($"The license expires within {ExpiryWarningDays} days after the flight.");
        }

        if (!context.LicenseCategories.Contains(context.Aircraft.Category))
        {
            failures.Add($"The license does not cover the {context.Aircraft.Category} category.");
        }

        return RuleResult.From("License", failures, warnings, "The license is valid for this flight.");
    }
}