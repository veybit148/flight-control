using System.Text.RegularExpressions;
using FlightControl.Validation.Core;

namespace FlightControl.Validation.Validators.Fleets;

internal static partial class AircraftRules
{
    private const string RegistrationField = "RegistrationNumber";
    private const string AltitudeField = "MaxAltitudeMeters";

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9-]{2,31}$")]
    private static partial Regex RegistrationNumberRegex();

    public static void RegistrationNumber(string? value, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(RegistrationField, "Registration number can't be empty.");
        }
        else if (!RegistrationNumberRegex().IsMatch(value.Trim()))
        {
            result.AddError(RegistrationField,
                "Registration number must be 3 to 32 letters, digits or hyphens, starting with a letter or digit.");
        }
    }

    public static void MaxAltitude(double value, ValidationResult result)
    {
        if (value <= 0)
        {
            result.AddError(AltitudeField, "Max altitude must be positive.");
        }
    }

    public static bool IsEnumName<TEnum>(string? value) where TEnum : struct, Enum =>
        value is not null &&
        Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase);

    public static string EnumNames<TEnum>() where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetNames<TEnum>());
}   