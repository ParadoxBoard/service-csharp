using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using service_csharp.Models;
using service_csharp.Services;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly IProjectTemplateService _templateService;

    public TemplatesController(IProjectTemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <summary>
    /// Obtener todas las plantillas de metodologías ágiles disponibles
    /// </summary>
    [HttpGet]
    public ActionResult<List<ProjectTemplate>> GetAllTemplates()
    {
        var templates = _templateService.GetAllTemplates();
        return Ok(templates);
    }

    /// <summary>
    /// Obtener una plantilla específica por nombre
    /// </summary>
    [HttpGet("{name}")]
    public ActionResult<ProjectTemplate> GetTemplateByName(string name)
    {
        var template = _templateService.GetTemplateByName(name);
        if (template == null)
        {
            return NotFound(new { message = $"Plantilla '{name}' no encontrada" });
        }
        return Ok(template);
    }

    /// <summary>
    /// Crear un proyecto desde una plantilla de metodología ágil
    /// </summary>
    [HttpPost("create-project")]
    public async Task<ActionResult<Project>> CreateProjectFromTemplate([FromBody] CreateProjectFromTemplateRequest request)
    {
        try
        {
            // Obtener el userId del token JWT
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var project = await _templateService.CreateProjectFromTemplateAsync(request, userId);
            return CreatedAtAction(nameof(GetTemplateByName), new { name = request.TemplateName }, project);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
