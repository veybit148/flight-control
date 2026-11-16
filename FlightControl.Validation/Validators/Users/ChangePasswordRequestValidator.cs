using FlightControl.Dto.Auth;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Users;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    protected override void ValidateRules(ChangePasswordRequest instance, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(instance.CurrentPassword))
        {
            result.AddError(nameof(instance.CurrentPassword), "Current password can't be empty.");
        }

        PasswordRules.Password(instance.NewPassword, nameof(instance.NewPassword), result);

        if (!string.IsNullOrEmpty(instance.NewPassword) &&
            string.Equals(instance.NewPassword, instance.CurrentPassword, StringComparison.Ordinal))
        {
            result.AddError(nameof(instance.NewPassword), "New password must differ from the current password.");
        }
    }
}
