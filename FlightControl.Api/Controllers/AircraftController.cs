using FlightControl.Domain.Operations.Fleets;
using FlightControl.Dto.Fleets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightControl.Api.Controllers;

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
    public async Task<IActionResult> Create(CreateAircraftRequest request)
    {
        var response = await _aircraftOperation.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AircraftQuery query)
    {
        var response = await _aircraftOperation.ListAsync(query);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _aircraftOperation.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = OwnerRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateAircraftRequest request)
    {
        var response = await _aircraftOperation.UpdateAsync(id, request);
        return Ok(response);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeAircraftStatusRequest request)
    {
        var response = await _aircraftOperation.ChangeStatusAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = OwnerRoles)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _aircraftOperation.DeleteAsync(id);
        return NoContent();
    }
}