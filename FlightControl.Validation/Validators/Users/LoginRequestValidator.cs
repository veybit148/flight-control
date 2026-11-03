using FlightControl.Dto.Users;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Users;

public class LoginRequestValidator: AbstractValidator<LoginRequest>
{
    protected override void ValidateRules(LoginRequest instance, ValidationResult result)
    {
        if(string.IsNullOrWhiteSpace(instance.Email) )
        {
            result.AddError(nameof(instance.Email), "Email can't be empty.");
        }
        if (string.IsNullOrWhiteSpace(instance.Password))
        {
            result.AddError(nameof(instance.Password), "Password can't be empty.");          
        }
    }
}