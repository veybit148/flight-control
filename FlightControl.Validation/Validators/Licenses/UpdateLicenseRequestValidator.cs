using FlightControl.Dto.Licenses;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Licenses;

public class UpdateLicenseRequestValidator : AbstractValidator<UpdateLicenseRequest>
{
    protected override void ValidateRules(UpdateLicenseRequest instance, ValidationResult result)
    {
        LicenseRules.ExpiresAt(instance.ExpiresAt, result);
        LicenseRules.Categories(instance.Categories, result);
    }
}