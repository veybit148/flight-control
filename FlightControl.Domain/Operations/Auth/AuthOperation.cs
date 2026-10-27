using FlightControl.Dal.Entities;
using FlightControl.Domain.Exceptions;
using FlightControl.Domain.Mappings;
using FlightControl.Domain.Security;
using FlightControl.Dto.Users;
using FlightControl.Repository.Interfaces;
using FlightControl.Validation.Core;

namespace FlightControl.Domain.Operations.Auth;

public class AuthOperation : IAuthOperation
{
    private readonly AbstractValidator<RegisterRequest> _validator;
    private readonly IUserRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthOperation(
        AbstractValidator<RegisterRequest> validator,
        IUserRepository repository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _repository = repository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        _validator.ThrowIfInvalid(request);
            
        RegisterRequest normalizedRequest = 
        request with {Email = request.Email.Trim().ToLowerInvariant()};

        if(await _repository.ExistsByEmailAsync(normalizedRequest.Email))
        {
            throw new ConflictException("This email is already in use.");
        }   

        string hashedPassword = _passwordService.HashPassword(request.Password);

        User user = normalizedRequest.ToEntity(hashedPassword);

        _repository.Add(user);

        await _unitOfWork.SaveChangesAsync();

        return user.ToResponse();
    }
}