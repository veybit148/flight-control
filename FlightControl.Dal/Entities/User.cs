using FlightControl.Dal.Enums;

namespace FlightControl.Dal.Entities;

public class User : BaseEntity
{
    public required string Email {get; set;}
    public required string PasswordHash {get; set;}
    public required string FullName {get; set;}
    public required UserRole Role {get; set;}
}