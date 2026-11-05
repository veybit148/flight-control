namespace FlightControl.Domain.Exceptions;

public class InvalidCredentialsException : UnauthorizedException
{
    public InvalidCredentialsException(string message) : base(message){}
}