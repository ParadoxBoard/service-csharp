using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service_csharp.Data;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ParadoxContext _context;

    public HealthController(ParadoxContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Health check endpoint - No requiere autenticación
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Verificar conexión a la base de datos
            await _context.Database.CanConnectAsync();
            
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "service-csharp",
                database = "connected"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow,
                service = "service-csharp",
                database = "disconnected",
                error = ex.Message
            });
        }
    }
}
