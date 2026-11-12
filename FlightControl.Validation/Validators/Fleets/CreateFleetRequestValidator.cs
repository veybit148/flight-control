using FlightControl.Dto.Fleets;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Fleets;

public class CreateFleetRequestValidator : AbstractValidator<CreateFleetRequest>
{
    private const int NameMaxLength = 256;

    protected override void ValidateRules(CreateFleetRequest instance, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(instance.Name))
        {
            result.AddError(nameof(instance.Name), "Fleet name can't be empty.");
        }
        else if (instance.Name.Trim().Length > NameMaxLength)
        {
            result.AddError(nameof(instance.Name), $"Fleet name can't be longer than {NameMaxLength} characters.");
        }
    }
}