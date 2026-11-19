using FlightControl.Domain.Operations.Permits;
using FlightControl.Dto.Common;
using FlightControl.Dto.Permits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightControl.Api.Controllers;

// 401 and 403 responses produced by the framework (JwtBearer, [Authorize(Roles)]) have an
// empty body, so they are declared with typeof(void); see DESIGN_DECISIONS item 15.
[Route("api/permits")]
[ApiController]
[Authorize]
public class PermitsController : ControllerBase
{
    private const string RequesterRoles = "Pilot,FleetOwner,Military";
    private const string AuthorityRole = "Authority";

    private readonly IFlightPermitOperation _permitOperation;

    public PermitsController(IFlightPermitOperation permitOperation)
    {
        _permitOperation = permitOperation;
    }

    [HttpPost]
    [Authorize(Roles = RequesterRoles)]
    [ProducesResponseType<PermitResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreatePermitRequest request)
    {
        var response = await _permitOperation.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResponse<PermitResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] PermitQuery query) =>
        Ok(await _permitOperation.ListAsync(query));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PermitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await _permitOperation.GetByIdAsync(id));

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = RequesterRoles)]
    [ProducesResponseType<PermitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id) =>
        Ok(await _permitOperation.CancelAsync(id));

    // 409 when a pre-check fails or the request is no longer pending;
    // 400 when a pre-check warns and no note is given.
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType<PermitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id, ApprovePermitRequest request) =>
        Ok(await _permitOperation.ApproveAsync(id, request));

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = AuthorityRole)]
    [ProducesResponseType<PermitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(Guid id, RejectPermitRequest request) =>
        Ok(await _permitOperation.RejectAsync(id, request));
}
