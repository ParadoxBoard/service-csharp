using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint - No requiere autenticación
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "service-csharp"
        });
    }
}
