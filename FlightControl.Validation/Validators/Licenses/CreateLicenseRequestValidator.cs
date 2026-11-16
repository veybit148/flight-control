using FlightControl.Dto.Licenses;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Licenses;

public class CreateLicenseRequestValidator : AbstractValidator<CreateLicenseRequest>
{
    protected override void ValidateRules(CreateLicenseRequest instance, ValidationResult result)
    {
        LicenseRules.LicenseNumber(instance.LicenseNumber, result);
        LicenseRules.ExpiresAt(instance.ExpiresAt, result);
        LicenseRules.Categories(instance.Categories, result);
    }
}