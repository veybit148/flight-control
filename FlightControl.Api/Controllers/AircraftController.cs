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
public class AircraftController : ControllerBase
{
    private const string OwnerRoles = "Pilot,FleetOwner,Military";

    private readonly IAircraftOperation _aircraftOperation;

    public AircraftController(IAircraftOperation aircraftOperation)
    {
        _aircraftOperation = aircraftOperation;
    }

    [HttpPost]
    [Authorize(Roles = OwnerRoles)]
    [ProducesResponseType<AircraftResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateAircraftRequest request)
    {
        var response = await _aircraftOperation.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AircraftResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] AircraftQuery query)
    {
        var response = await _aircraftOperation.ListAsync(query);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AircraftResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _aircraftOperation.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = OwnerRoles)]
    [ProducesResponseType<AircraftResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateAircraftRequest request)
    {
        var response = await _aircraftOperation.UpdateAsync(id, request);
        return Ok(response);
    }

    [HttpPost("{id:guid}/status")]
    [ProducesResponseType<AircraftResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeAircraftStatusRequest request)
    {
        var response = await _aircraftOperation.ChangeStatusAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = OwnerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _aircraftOperation.DeleteAsync(id);
        return NoContent();
    }
}
