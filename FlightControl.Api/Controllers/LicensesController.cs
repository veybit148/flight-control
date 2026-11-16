using FlightControl.Domain.Operations.Licenses;
using FlightControl.Dto.Licenses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightControl.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LicensesController : ControllerBase
{
    private const string PilotRole = "Pilot";
    private const string AuthorityRole = "Authority";

    private readonly ILicenseOperation _licenseOperation;

    public LicensesController(ILicenseOperation licenseOperation)
    {
        _licenseOperation = licenseOperation;
    }

    [HttpPost]
    [Authorize(Roles = PilotRole)]
    public async Task<IActionResult> Create(CreateLicenseRequest request)
    {
        var response = await _licenseOperation.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        return Ok(await _licenseOperation.ListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _licenseOperation.GetByIdAsync(id));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = PilotRole)]
    public async Task<IActionResult> Update(Guid id, UpdateLicenseRequest request)
    {
        return Ok(await _licenseOperation.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = PilotRole)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _licenseOperation.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = AuthorityRole)]
    public async Task<IActionResult> Suspend(Guid id, SuspendLicenseRequest request)
    {
        return Ok(await _licenseOperation.SuspendAsync(id, request));
    }

    [HttpPost("{id:guid}/reinstate")]
    [Authorize(Roles = AuthorityRole)]
    public async Task<IActionResult> Reinstate(Guid id)
    {
        return Ok(await _licenseOperation.ReinstateAsync(id));
    }
}