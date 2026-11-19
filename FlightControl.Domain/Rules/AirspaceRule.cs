using FlightControl.Dal.Enums;

namespace FlightControl.Domain.Rules;

public class AirspaceRule : IPermitRule
{
    public RuleResult Check(PermitCheckContext context)
    {
        var permit = context.Permit;
        List<string> failures = [];
        List<string> warnings = [];

        foreach (var zone in context.IntersectingZones)
        {
            // Flights start at ground level; a zone whose floor is at or above the flight's ceiling is not entered.
            if (zone.MinAltitudeMeters >= permit.MaxAltitudeMeters)
            {
                continue;
            }

            switch (zone.Type)
            {
                case ZoneType.Restricted:
                    failures.Add($"The area enters the restricted zone '{zone.Name}'.");
                    break;
                case ZoneType.Military when permit.Type != PermitRequestType.Military:
                    failures.Add($"The area enters the military zone '{zone.Name}'.");
                    break;
                case ZoneType.Controlled:
                    warnings.Add($"The area enters the controlled zone '{zone.Name}'.");
                    break;
            }

            if (zone.Type != ZoneType.Free && permit.MaxAltitudeMeters > zone.MaxAltitudeMeters)
            {
                warnings.Add($"The flight goes above the upper limit of '{zone.Name}'.");
            }
        }

        return RuleResult.From("Airspace", failures, warnings, "The area does not enter any restricted airspace.");
    }
}