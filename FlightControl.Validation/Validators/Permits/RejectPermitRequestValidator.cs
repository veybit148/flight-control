using FlightControl.Dto.Permits;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Permits;

public class RejectPermitRequestValidator : AbstractValidator<RejectPermitRequest>
{
    private const int ReasonMaxLength = 1000;

    protected override void ValidateRules(RejectPermitRequest instance, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(instance.Reason))
        {
            result.AddError(nameof(instance.Reason), "A reason is required to reject a request.");
        }
        else if (instance.Reason.Length > ReasonMaxLength)
        {
            result.AddError(nameof(instance.Reason), $"Reason can't be longer than {ReasonMaxLength} characters.");
        }
    }
}