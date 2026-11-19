namespace FlightControl.Domain.Rules;

public class ConflictRule : IPermitRule
{
    public RuleResult Check(PermitCheckContext context)
    {
        var permit = context.Permit;
        List<string> failures = [];
        List<string> warnings = [];

        if (context.TimeOverlappingApproved.Any(p => p.AircraftId == permit.AircraftId))
        {
            failures.Add("The aircraft already has an approved permit for overlapping times.");
        }

        if (context.TimeOverlappingApproved.Any(p => p.PilotId == permit.PilotId))
        {
            failures.Add("The pilot already has an approved permit for overlapping times.");
        }

        int areaConflicts = context.AreaAndTimeOverlappingApproved
            .Count(p => p.AircraftId != permit.AircraftId && p.PilotId != permit.PilotId);

        if (areaConflicts > 0)
        {
            warnings.Add($"{areaConflicts} other approved flight(s) use the same area at overlapping times.");
        }

        return RuleResult.From("Conflict", failures, warnings, "No conflicting approved flights.");
    }
}