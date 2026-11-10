namespace FlightControl.Dal.Entities;

public class RefreshToken : BaseEntity
{
    public string TokenHash {get; set;} = string.Empty;
    public Guid UserId {get; set;}

    public DateTime ExpiresAt {get; set;}

    public DateTime SessionExpiresAt {get; set;}

    public DateTime? RevokedAt {get; set;}
}