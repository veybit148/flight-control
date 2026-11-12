using FlightControl.Dal.Enums;
using FlightControl.Dto.Fleets;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Fleets;

public class CreateAircraftRequestValidator : AbstractValidator<CreateAircraftRequest>
{
    protected override void ValidateRules(CreateAircraftRequest instance, ValidationResult result)
    {
        AircraftRules.RegistrationNumber(instance.RegistrationNumber, result);
        AircraftRules.MaxAltitude(instance.MaxAltitudeMeters, result);

        if (!AircraftRules.IsEnumName<AircraftCategory>(instance.Category))
        {
            result.AddError(nameof(instance.Category),
                $"Category must be one of: {AircraftRules.EnumNames<AircraftCategory>()}.");
        }
    }
}