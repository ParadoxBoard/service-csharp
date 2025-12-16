using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/tasks/sync")]
public class SyncController : ControllerBase
{
    private readonly ILogger<SyncController> _logger;

    public SyncController(ILogger<SyncController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult ReceiveSyncEvent([FromBody] JsonElement payload)
    {
        try
        {
            _logger.LogInformation("Received sync event: {Payload}", payload.ToString());

            if (payload.TryGetProperty("event", out var eventType))
            {
                _logger.LogInformation("Event Type: {EventType}", eventType.GetString());
                
                // Here you would implement the logic to handle different event types
                // e.g., commit.created, pull_request.opened, etc.
                // For MVP, logging is sufficient to prove integration.
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sync event");
            return StatusCode(500, "Internal server error");
        }
    }
}
