using FlightControl.Dto.Permits;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Permits;

public class ApprovePermitRequestValidator : AbstractValidator<ApprovePermitRequest>
{
    private const int NoteMaxLength = 1000;

    protected override void ValidateRules(ApprovePermitRequest instance, ValidationResult result)
    {
        if (instance.Note is { Length: > NoteMaxLength })
        {
            result.AddError(nameof(instance.Note), $"Note can't be longer than {NoteMaxLength} characters.");
        }
    }
}