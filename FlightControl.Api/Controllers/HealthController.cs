using Microsoft.AspNetCore.Mvc;
using FlightControl.Domain.Exceptions;
using FlightControl.Validation.Core;

namespace FlightControl.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase 
{
    [HttpGet]
    public IActionResult CheckStatus()
    {
        return Ok("Healthy");
    }

    [HttpGet("error")]
    public IActionResult TestError(string type)
    {
        if(type == "notfound")
        {
            throw new NotFoundException("Test: aircraft not found.");
        }
        if(type == "validation")
        {

            ValidationResult result = new ValidationResult();
            result.AddError("a", "a1");
            result.AddError("a", "a2");
            result.AddError("b", "b1");

            throw new ValidationException(result);
        }
        if(type == "conflict")
        {
            throw new ConflictException("Test: aircraft already exists.");
        }
        if(type == "general")
        {
            throw new Exception("Test: general exception.");
        }
        
        return Ok();
    }
}







