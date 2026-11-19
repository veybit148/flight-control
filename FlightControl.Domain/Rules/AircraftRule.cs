using FlightControl.Dal.Enums;

namespace FlightControl.Domain.Rules;

public class AircraftRule : IPermitRule
{
    private const int MaintenanceWarningDays = 7;

    public RuleResult Check(PermitCheckContext context)
    {
        var permit = context.Permit;
        var aircraft = context.Aircraft;
        List<string> failures = [];
        List<string> warnings = [];

        if (aircraft.Status == AircraftStatus.Grounded)
        {
            failures.Add("The aircraft is grounded.");
        }
        else if (aircraft.Status == AircraftStatus.Maintenance)
        {
            failures.Add("The aircraft is in maintenance.");
        }

        if (aircraft.NextMaintenanceDate < permit.PlannedEnd)
        {
            failures.Add("Maintenance is due before the flight ends.");
        }
        else if (aircraft.NextMaintenanceDate < permit.PlannedEnd.AddDays(MaintenanceWarningDays))
        {
            warnings.Add($"Maintenance is due within {MaintenanceWarningDays} days after the flight.");
        }

        if (permit.MaxAltitudeMeters > aircraft.MaxAltitudeMeters)
        {
            failures.Add($"The requested altitude is above the aircraft's limit of {aircraft.MaxAltitudeMeters} m.");
        }

        return RuleResult.From("Aircraft", failures, warnings, "The aircraft is fit for this flight.");
    }
}