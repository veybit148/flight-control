using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Dto.Enums;
using FlightControl.Dto.Users;

namespace FlightControl.Domain.Mappings;

public static class UserMappings
{
    
    public static User ToEntity(this RegisterRequest request, string passwordHash)
    {
       var user = new User
       {
            Email =  request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Role = request.Role.ToUserRole()
       };

       return user;
    }

    public static UserRole ToUserRole(this RegistrationRole role) => role switch
    {
      RegistrationRole.Pilot => UserRole.Pilot,
      RegistrationRole.FleetOwner => UserRole.FleetOwner,
      RegistrationRole.Military => UserRole.Military,
      _ => throw new ArgumentOutOfRangeException(nameof(role))
    };

    public static UserResponse ToResponse(this User user)
    {
        UserResponse response = new UserResponse
        (
            user.Id,
            user.Email,
            user.FullName,
            user.CreatedAt,
            user.Role.ToString()
        );

        return response;
    }
}