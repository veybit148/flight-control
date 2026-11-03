using FlightControl.Domain.Operations.Auth;
using FlightControl.Dto.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightControl.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthOperation _authOperation;

    public AuthController(IAuthOperation authOperation)
    {
        _authOperation = authOperation;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
       var response = await _authOperation.RegisterAsync(request);
       return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
       var response = await _authOperation.LoginAsync(request);
       return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
       var userClaims = User.Claims.Select(c => new 
       {
           Type = c.Type,
           Value = c.Value
       }).ToList();

       return Ok(userClaims);
    }

}