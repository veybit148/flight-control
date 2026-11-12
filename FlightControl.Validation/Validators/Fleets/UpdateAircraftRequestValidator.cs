using FlightControl.Dto.Fleets;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Fleets;

public class UpdateAircraftRequestValidator : AbstractValidator<UpdateAircraftRequest>
{
    protected override void ValidateRules(UpdateAircraftRequest instance, ValidationResult result)
    {
        AircraftRules.RegistrationNumber(instance.RegistrationNumber, result);
        AircraftRules.MaxAltitude(instance.MaxAltitudeMeters, result);
    }
}