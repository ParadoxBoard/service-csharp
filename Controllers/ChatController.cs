using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        var userId = User.GetUserId();
        var conversation = await _aiService.CreateConversationAsync(request.ProjectId, userId, request.Title);
        return Ok(conversation);
    }

    [HttpPost("conversations/{id:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request)
    {
        var userId = User.GetUserId();
        var response = await _aiService.SendMessageAsync(id, request.Message, userId);
        return Ok(new { response });
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] Guid? projectId = null)
    {
        var userId = User.GetUserId();
        var conversations = await _aiService.GetConversationsAsync(userId, projectId);
        return Ok(conversations);
    }

    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, [FromQuery] int take = 50, [FromQuery] int skip = 0)
    {
        var userId = User.GetUserId();
        var messages = await _aiService.GetMessagesAsync(id, userId, take, skip);
        return Ok(messages);
    }
}

public record CreateConversationRequest(Guid? ProjectId, string? Title);
public record SendMessageRequest(string Message);

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return id != null ? Guid.Parse(id) : throw new UnauthorizedAccessException("User id claim not found");
    }
}
