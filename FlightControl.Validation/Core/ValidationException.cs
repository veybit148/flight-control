namespace FlightControl.Validation.Core;

public class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors {get;}
    public ValidationException(ValidationResult result) : base("Validation Failed")
    {
        Errors = result.Errors;
    }
}