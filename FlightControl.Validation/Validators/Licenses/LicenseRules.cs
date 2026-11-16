using System.Text.RegularExpressions;
using FlightControl.Dal.Enums;
using FlightControl.Validation.Core;
using FlightControl.Validation.Validators.Fleets;

namespace FlightControl.Validation.Validators.Licenses;

internal static partial class LicenseRules
{
    private const string NumberField = "LicenseNumber";
    private const string ExpiresField = "ExpiresAt";
    private const string CategoriesField = "Categories";

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9-]{2,31}$")]
    private static partial Regex LicenseNumberRegex();

    public static void LicenseNumber(string? value, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(NumberField, "License number can't be empty.");
        }
        else if (!LicenseNumberRegex().IsMatch(value.Trim()))
        {
            result.AddError(NumberField,
                "License number must be 3 to 32 letters, digits or hyphens, starting with a letter or digit.");
        }
    }

    public static void ExpiresAt(DateTime value, ValidationResult result)
    {
        if (value <= DateTime.UtcNow)
        {
            result.AddError(ExpiresField, "Expiry date can't be in the past.");
        }
    }

    public static void Categories(IReadOnlyList<string>? values, ValidationResult result)
    {
        if (values == null || values.Count == 0)
        {
            result.AddError(CategoriesField, "At least one category must be selected.");
            return;
        }

        if (values.Any(v => !AircraftRules.IsEnumName<AircraftCategory>(v)))
        {
            result.AddError(CategoriesField,
                $"Each category must be one of: {AircraftRules.EnumNames<AircraftCategory>()}.");
        }
        else if (values.Distinct(StringComparer.OrdinalIgnoreCase).Count() != values.Count)
        {
            result.AddError(CategoriesField, "A category can't be selected twice.");
        }
    }
}