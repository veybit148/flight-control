using FlightControl.Dal.Enums;
using FlightControl.Dto.Permits;
using FlightControl.Validation.Core;
using FlightControl.Validation.Validators.Fleets;

namespace FlightControl.Validation.Validators.Permits;

public class PermitQueryValidator : AbstractValidator<PermitQuery>
{
    private const int MaxPageSize = 100;
    private static readonly string[] SortOptions = ["start", "militaryFirst"];

    protected override void ValidateRules(PermitQuery instance, ValidationResult result)
    {
        if(instance.Status is not null && !AircraftRules.IsEnumName<PermitStatus>(instance.Status))
        {
            result.AddError(nameof(instance.Status), $"Status must be one of: {AircraftRules.EnumNames<PermitStatus>()}.");
        }

        if(instance.Type is not null && !AircraftRules.IsEnumName<PermitRequestType>(instance.Type))
        {
            result.AddError(nameof(instance.Type), $"Type must be one of: {AircraftRules.EnumNames<PermitRequestType>()}.");
        }

        if(instance.From is { } from && instance.To is { } to && from >= to)
        {
            result.AddError(nameof(instance.To), "The end of the date range must be after its start.");
        }

        if(instance.Page < 1)
        {
            result.AddError(nameof(instance.Page), "Page must be 1 or greater.");
        }

        if(instance.PageSize is < 1 or > MaxPageSize)
        {
            result.AddError(nameof(instance.PageSize), $"Page size must be between 1 and {MaxPageSize}.");
        }
        if(instance.Sort is not null && !SortOptions.Contains(instance.Sort, StringComparer.OrdinalIgnoreCase))
        {
            result.AddError(nameof(instance.Sort), $"Sort must be one of: {string.Join(", ", SortOptions)}.");
        }
    }
}