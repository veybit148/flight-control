using FlightControl.Dal.Enums;
using FlightControl.Dal.Spatial;
using FlightControl.Dto.Airspace;
using FlightControl.Validation.Core;
using FlightControl.Validation.Validators.Fleets;

namespace FlightControl.Validation.Validators.Airspace;

public class AirspaceZoneRequestValidator : AbstractValidator<AirspaceZoneRequest>
{
    private const int NameMaxLength = 256;

    protected override void ValidateRules(AirspaceZoneRequest instance, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(instance.Name))
        {
            result.AddError(nameof(instance.Name), "Zone name can't be empty.");
        }
        else if (instance.Name.Trim().Length > NameMaxLength)
        {
            result.AddError(nameof(instance.Name), $"Zone name can't be longer than {NameMaxLength} characters.");
        }

        if (!AircraftRules.IsEnumName<ZoneType>(instance.Type))
        {
            result.AddError(nameof(instance.Type), $"Type must be one of: {AircraftRules.EnumNames<ZoneType>()}.");
        }

        ValidateArea(instance.Area, result);

        if (instance.MinAltitudeMeters < 0)
        {
            result.AddError(nameof(instance.MinAltitudeMeters), "Lower altitude limit can't be negative.");
        }

        if (instance.MaxAltitudeMeters <= instance.MinAltitudeMeters)
        {
            result.AddError(nameof(instance.MaxAltitudeMeters), "Upper altitude limit must be above the lower limit.");
        }

        if (instance.ActiveFrom is { } from && instance.ActiveUntil is { } until && from >= until)
        {
            result.AddError(nameof(instance.ActiveUntil), "The active period must end after it starts.");
        }
    }

    internal static void ValidateArea(IReadOnlyList<CoordinateDto>? area, ValidationResult result)
    {
        const string field = nameof(AirspaceZoneRequest.Area);

        if (area == null || area.Distinct().Count() < 3)
        {
            result.AddError(field, "An area needs at least three distinct corners.");
            return;
        }

        if (area.Any(p => p.Latitude is < -90 or > 90 || p.Longitude is < -180 or > 180))
        {
            result.AddError(field, "Latitude must be between -90 and 90, longitude between -180 and 180.");
            return;
        }

        var polygon = Geo.CreatePolygon(area.Select(p => (p.Latitude, p.Longitude)));

        if (!polygon.IsValid)
        {
            result.AddError(field, "The area is not a valid polygon; its edges must not cross each other.");
        }
    }
}