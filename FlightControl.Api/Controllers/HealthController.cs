using Microsoft.AspNetCore.Mvc;
using FlightControl.Domain.Exceptions;
using FlightControl.Validation.Core;

namespace FlightControl.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase 
{
    [HttpGet]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public IActionResult CheckStatus()
    {
        return Ok("Healthy");
    }

    // Throws the requested exception on purpose to test ExceptionHandlingMiddleware.
    [HttpGet("error")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
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







