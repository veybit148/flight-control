namespace FlightControl.Validation.Core;

public abstract class AbstractValidator<T>
{

    public ValidationResult Validate(T instance)
    {
        var result = new ValidationResult();
        ValidateRules(instance, result);

        return result;
    }

    protected abstract void ValidateRules(T instance, ValidationResult result);    
}