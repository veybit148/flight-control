using FlightControl.Dto.Licenses;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Licenses;

public class SuspendLicenseRequestValidator : AbstractValidator<SuspendLicenseRequest>
{
    private const int ReasonMaxLength = 500;

    protected override void ValidateRules(SuspendLicenseRequest instance, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(instance.Reason))
        {
            result.AddError(nameof(instance.Reason), "A reason is required to suspend a license.");
        }
        else if (instance.Reason.Length > ReasonMaxLength)
        {
            result.AddError(nameof(instance.Reason), $"Reason can't be longer than {ReasonMaxLength} characters.");
        }
    }
}