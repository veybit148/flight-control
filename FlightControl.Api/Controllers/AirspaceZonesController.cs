using FlightControl.Domain.Operations.Airspace;
using FlightControl.Dto.Airspace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightControl.Api.Controllers;

// 401 and 403 responses produced by the framework (JwtBearer, [Authorize(Roles)]) have an
// empty body, so they are declared with typeof(void); see DESIGN_DECISIONS item 15.
[Route("api/airspace-zones")]
[ApiController]
[Authorize]
public class AirspaceZonesController : ControllerBase
{
    private const string AuthorityRole = "Authority";

    private readonly IAirspaceZoneOperation _zoneOperation;

    public AirspaceZonesController(IAirspaceZoneOperation zoneOperation)
    {
        _zoneOperation = zoneOperation;
    }

    [HttpPost]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType<AirspaceZoneResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(AirspaceZoneRequest request)
    {
        var response = await _zoneOperation.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AirspaceZoneResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List() => Ok(await _zoneOperation.ListAsync());

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AirspaceZoneResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _zoneOperation.GetByIdAsync(id));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType<AirspaceZoneResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, AirspaceZoneRequest request) =>
        Ok(await _zoneOperation.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _zoneOperation.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("at-point")]
    [ProducesResponseType<IReadOnlyList<AirspaceZoneResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAtPoint([FromQuery] ZonesAtPointQuery query) =>
        Ok(await _zoneOperation.GetAtPointAsync(query));
}
