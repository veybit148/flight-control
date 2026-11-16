using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Users;

// Shared by registration and password change, so both accept exactly the same passwords.
internal static class PasswordRules
{
    private const int MinLength = 8;
    private const int MaxLength = 100;

    public static void Password(string? value, string field, ValidationResult result)
    {
        if(string.IsNullOrWhiteSpace(value))
        {
            result.AddError(field, "Password can't be empty");
            return;
        }

        if(value.Length > MaxLength)
        {
            result.AddError(field, $"Password length can't exceed {MaxLength} characters");
        }

        if(value.Length < MinLength)
        {
            result.AddError(field, $"Password length must be at least {MinLength} characters");
        }
    }
}
