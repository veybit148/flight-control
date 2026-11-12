using FlightControl.Domain.Operations.Fleets;
using FlightControl.Dto.Fleets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightControl.Api.Controllers;

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
    public async Task<IActionResult> Create(CreateFleetRequest request)
    {
        var response = await _fleetOperation.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var response = await _fleetOperation.ListAsync();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _fleetOperation.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = FleetOwnerRoles)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _fleetOperation.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Roles = FleetOwnerRoles)]
    public async Task<IActionResult> AddMember(Guid id, AddFleetMemberRequest request)
    {
        var response = await _fleetOperation.AddMemberAsync(id, request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> ListMembers(Guid id)
    {
        var response = await _fleetOperation.ListMembersAsync(id);
        return Ok(response);
    }

    [HttpDelete("{id:guid}/members/{pilotId:guid}")]
    [Authorize(Roles = FleetOwnerRoles)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid pilotId)
    {
        await _fleetOperation.RemoveMemberAsync(id, pilotId);
        return NoContent();
    }
}