using FlightControl.Dto.Airspace;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Airspace;

public class ZonesAtPointQueryValidator : AbstractValidator<ZonesAtPointQuery>
{
    protected override void ValidateRules(ZonesAtPointQuery instance, ValidationResult result)
    {
        if (instance.Latitude is < -90 or > 90)
        {
            result.AddError(nameof(instance.Latitude), "Latitude must be between -90 and 90.");
        }

        if (instance.Longitude is < -180 or > 180)
        {
            result.AddError(nameof(instance.Longitude), "Longitude must be between -180 and 180.");
        }

        if (instance.AltitudeMeters < 0)
        {
            result.AddError(nameof(instance.AltitudeMeters), "Altitude can't be negative.");
        }
    }
}