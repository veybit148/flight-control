using FlightControl.Dto.Permits;
using FlightControl.Validation.Core;
using FlightControl.Validation.Validators.Airspace;

namespace FlightControl.Validation.Validators.Permits;

public class CreatePermitRequestValidator : AbstractValidator<CreatePermitRequest>
{
    private const int PurposeMaxLength = 500;

    protected override void ValidateRules(CreatePermitRequest instance, ValidationResult result)
    {
        if (instance.AircraftId == Guid.Empty)
        {
            result.AddError(nameof(instance.AircraftId), "Aircraft is required.");
        }

        if (instance.LicenseId == Guid.Empty)
        {
            result.AddError(nameof(instance.LicenseId), "License is required.");
        }

        AirspaceZoneRequestValidator.ValidateArea(instance.Area, result);

        if (instance.PlannedStart <= DateTime.UtcNow)
        {
            result.AddError(nameof(instance.PlannedStart), "The flight must start in the future.");
        }

        if (instance.PlannedEnd <= instance.PlannedStart)
        {
            result.AddError(nameof(instance.PlannedEnd), "The flight must end after it starts.");
        }

        if (instance.MaxAltitudeMeters <= 0)
        {
            result.AddError(nameof(instance.MaxAltitudeMeters), "Max altitude must be positive.");
        }

        if (string.IsNullOrWhiteSpace(instance.Purpose))
        {
            result.AddError(nameof(instance.Purpose), "Purpose can't be empty.");
        }
        else if (instance.Purpose.Trim().Length > PurposeMaxLength)
        {
            result.AddError(nameof(instance.Purpose), $"Purpose can't be longer than {PurposeMaxLength} characters.");
        }
    }
}