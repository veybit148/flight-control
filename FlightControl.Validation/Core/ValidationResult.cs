namespace FlightControl.Validation.Core;

public class ValidationResult
{
    private readonly List<ValidationError> _errors = new();
    public IReadOnlyList<ValidationError> Errors => _errors;

    public bool IsValid => _errors.Count == 0;

    public void AddError(string field, string message)
    {
        _errors.Add(new ValidationError(field, message));
    }
}
