using Microsoft.AspNetCore.Mvc;

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
}







