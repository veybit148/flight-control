namespace FlightControl.Domain.Security;

public class RefreshTokenOptions
{
    public int SlidingLifetimeDays {get; set;}
    public int AbsoluteLifetimeDays {get; set;}
}
