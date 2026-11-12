namespace FlightControl.Domain.Mappings;

public static class EnumMappings
{
    public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, Enum =>
        Enum.Parse<TEnum>(value, ignoreCase: true);
}