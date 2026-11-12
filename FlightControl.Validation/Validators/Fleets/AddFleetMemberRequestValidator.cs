using FlightControl.Dto.Fleets;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Fleets;

public class AddFleetMemberRequestValidator : AbstractValidator<AddFleetMemberRequest>
{
    private const int EmailMaxLength = 256;

    protected override void ValidateRules(AddFleetMemberRequest instance, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(instance.PilotEmail))
        {
            result.AddError(nameof(instance.PilotEmail), "Pilot email can't be empty.");
        }
        else if (instance.PilotEmail.Length > EmailMaxLength)
        {
            result.AddError(nameof(instance.PilotEmail), $"Pilot email can't be longer than {EmailMaxLength} characters.");
        }
    }
}