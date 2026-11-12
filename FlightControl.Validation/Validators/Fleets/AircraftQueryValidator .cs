using FlightControl.Dal.Enums;
using FlightControl.Dto.Fleets;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Fleets;

public class AircraftQueryValidator : AbstractValidator<AircraftQuery>
{
    protected override void ValidateRules(AircraftQuery instance, ValidationResult result)
    {
        if (instance.Status is not null && !AircraftRules.IsEnumName<AircraftStatus>(instance.Status))
        {
            result.AddError(nameof(instance.Status),
                $"Status must be one of: {AircraftRules.EnumNames<AircraftStatus>()}.");
        }

        if (instance.Category is not null && !AircraftRules.IsEnumName<AircraftCategory>(instance.Category))
        {
            result.AddError(nameof(instance.Category),
                $"Category must be one of: {AircraftRules.EnumNames<AircraftCategory>()}.");
        }
    }
}