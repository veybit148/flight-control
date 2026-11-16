using System.Text.RegularExpressions;
using FlightControl.Dto.Auth;
using FlightControl.Dto.Users;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Users;

public partial class RegisterRequestValidator: AbstractValidator<RegisterRequest>
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    protected override void ValidateRules(RegisterRequest instance, ValidationResult result)
    {
        if(string.IsNullOrWhiteSpace(instance.Email))
        {
            result.AddError(nameof(instance.Email), "Email can't be empty");
        }
        else
        {
            if(instance.Email.Length > 256)
            {
                result.AddError(nameof(instance.Email), "Email length can't exceed 256 characters");
            }
            if(!EmailRegex().IsMatch(instance.Email.Trim()))
            {
                result.AddError(nameof(instance.Email), "Email format invalid");
            }
        }

        if(string.IsNullOrWhiteSpace(instance.FullName))
        {
            result.AddError(nameof(instance.FullName), "FullName can't be empty");
        }
        else if(instance.FullName.Length > 100)
        {
            result.AddError(nameof(instance.FullName), "Name can't exceed 100 characters");
        }
        
        PasswordRules.Password(instance.Password, nameof(instance.Password), result);

        if(!Enum.IsDefined(instance.Role))
        {
            result.AddError(nameof(instance.Role), "Role must be one of: Pilot, FleetOwner, Military.");
        }
    }
}