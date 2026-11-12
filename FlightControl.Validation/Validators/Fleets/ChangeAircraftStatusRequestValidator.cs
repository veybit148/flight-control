using FlightControl.Dal.Enums;
using FlightControl.Dto.Fleets;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Fleets;

public class ChangeAircraftStatusRequestValidator : AbstractValidator<ChangeAircraftStatusRequest>
{
    private const int ReasonMaxLength = 500;

    protected override void ValidateRules(ChangeAircraftStatusRequest instance, ValidationResult result)
    {
        if (!AircraftRules.IsEnumName<AircraftStatus>(instance.Status))
        {
            result.AddError(nameof(instance.Status),
                $"Status must be one of: {AircraftRules.EnumNames<AircraftStatus>()}.");
            return;
        }

        bool isGrounding = string.Equals(
            instance.Status, nameof(AircraftStatus.Grounded), StringComparison.OrdinalIgnoreCase);

        if (isGrounding && string.IsNullOrWhiteSpace(instance.Reason))
        {
            result.AddError(nameof(instance.Reason), "A reason is required when grounding an aircraft.");
        }
        else if (instance.Reason is { Length: > ReasonMaxLength })
        {
            result.AddError(nameof(instance.Reason),
                $"Reason can't be longer than {ReasonMaxLength} characters.");
        }
    }
}