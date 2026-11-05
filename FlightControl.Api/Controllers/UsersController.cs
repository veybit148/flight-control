using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FlightControl.Domain.Operations.Users;

namespace FlightControl.Api.Controllers;

[Route("api/[controller]/me")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserOperation _userOperation;

    public UsersController(IUserOperation userOperation)
    {
        _userOperation = userOperation;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
       var response = await _userOperation.GetMeAsync();
       return StatusCode(StatusCodes.Status200OK, response);
    }
}