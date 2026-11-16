using FlightControl.Domain.Operations.Fleets;
using FlightControl.Dto.Fleets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightControl.Api.Controllers;

// 401 and 403 responses produced by the framework (JwtBearer, [Authorize(Roles)]) have an
// empty body, so they are declared with typeof(void); see DESIGN_DECISIONS item 15.
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FleetsController : ControllerBase
{
    private const string FleetOwnerRoles = "FleetOwner,Military";

    private readonly IFleetOperation _fleetOperation;

    public FleetsController(IFleetOperation fleetOperation)
    {
        _fleetOperation = fleetOperation;
    }

    [HttpPost]
    [Authorize(Roles = FleetOwnerRoles)]
    [ProducesResponseType<FleetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateFleetRequest request)
    {
        var response = await _fleetOperation.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FleetResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List()
    {
        var response = await _fleetOperation.ListAsync();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<FleetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _fleetOperation.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = FleetOwnerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _fleetOperation.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Roles = FleetOwnerRoles)]
    [ProducesResponseType<FleetMemberResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMember(Guid id, AddFleetMemberRequest request)
    {
        var response = await _fleetOperation.AddMemberAsync(id, request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("{id:guid}/members")]
    [ProducesResponseType<IReadOnlyList<FleetMemberResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListMembers(Guid id)
    {
        var response = await _fleetOperation.ListMembersAsync(id);
        return Ok(response);
    }

    [HttpDelete("{id:guid}/members/{pilotId:guid}")]
    [Authorize(Roles = FleetOwnerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid pilotId)
    {
        await _fleetOperation.RemoveMemberAsync(id, pilotId);
        return NoContent();
    }
}
