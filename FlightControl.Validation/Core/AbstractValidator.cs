namespace FlightControl.Validation.Core;

public abstract class AbstractValidator<T>
{

    public ValidationResult Validate(T instance)
    {
        var result = new ValidationResult();
        ValidateRules(instance, result);

        return result;
    }

    public void ThrowIfInvalid(T instance)
    {
        ValidationResult validationResult = Validate(instance);
        if(!validationResult.IsValid) throw new ValidationException(validationResult);
    }  

    protected abstract void ValidateRules(T instance, ValidationResult result); 


}