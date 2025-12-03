using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using service_csharp.Services;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IAiService _aiService;

    public ChatController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var conversation = await _aiService.CreateConversationAsync(request.ProjectId, request.UserId, request.Title);
        return Ok(conversation);
    }

    [HttpPost("conversations/{id}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request)
    {
        var response = await _aiService.SendMessageAsync(id, request.Message, request.UserId);
        return Ok(new { response });
    }
}

public record CreateConversationRequest(Guid? ProjectId, Guid UserId, string? Title);
public record SendMessageRequest(string Message, Guid UserId);
