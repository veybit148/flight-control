using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FlightControl.Domain.Operations.Users;
using FlightControl.Dto.Users;

namespace FlightControl.Api.Controllers;

// 401 and 403 responses produced by the framework (JwtBearer, [Authorize(Roles)]) have an
// empty body, so they are declared with typeof(void); see DESIGN_DECISIONS item 15.
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private const string AuthorityRole = "Authority";

    private readonly IUserOperation _userOperation;

    public UsersController(IUserOperation userOperation)
    {
        _userOperation = userOperation;
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
       var response = await _userOperation.GetMeAsync();
       return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet("pending")]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListPending()
    {
        var response = await _userOperation.ListPendingAsync();
        return Ok(response);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var response = await _userOperation.ApproveAsync(id);
        return Ok(response);
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var response = await _userOperation.SuspendAsync(id);
        return Ok(response);
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reactivate(Guid id)
    {
        var response = await _userOperation.ReactivateAsync(id);
        return Ok(response);
    }
}
