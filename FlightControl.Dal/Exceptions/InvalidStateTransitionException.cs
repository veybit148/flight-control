namespace FlightControl.Dal.Exceptions;

public class InvalidStateTransitionException : Exception
{
    public InvalidStateTransitionException(string message) : base(message) { }
}