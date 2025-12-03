using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using service_csharp.Data;
using service_csharp.Models;

namespace service_csharp.Services;

public class OpenAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ParadoxContext _context;
    private readonly IConfiguration _configuration;
    private readonly BoardTools _boardTools;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAiService(
        HttpClient httpClient, 
        ParadoxContext context, 
        IConfiguration configuration,
        BoardTools boardTools)
    {
        _httpClient = httpClient;
        _context = context;
        _configuration = configuration;
        _boardTools = boardTools;
        
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
        _model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4-turbo-preview";
    }

    public async Task<AiConversation> CreateConversationAsync(Guid? projectId, Guid userId, string? title = null)
    {
        var conversation = new AiConversation
        {
            ProjectId = projectId,
            CreatedBy = userId,
            Title = title ?? "New Conversation",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.AiConversations.Add(conversation);
        await _context.SaveChangesAsync();

        return conversation;
    }

    public async Task<string> SendMessageAsync(Guid conversationId, string message, Guid userId)
    {
        // 1. Guardar mensaje del usuario
        var userMsg = new AiMessage
        {
            ConversationId = conversationId,
            Role = "user",
            Content = JsonDocument.Parse(JsonSerializer.Serialize(new { text = message })),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.AiMessages.Add(userMsg);
        await _context.SaveChangesAsync();

        // 2. Preparar contexto para OpenAI (historial reciente)
        var history = await _context.AiMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Take(10) // Limitar contexto para no gastar tokens infinitos
            .ToListAsync();

        var messages = history.Select(m => new
        {
            role = m.Role,
            content = m.Content.RootElement.GetProperty("text").GetString()
        }).ToList();

        // 3. Definir herramientas disponibles
        var tools = new object[]
        {
            new {
                type = "function",
                function = new {
                    name = "create_issue",
                    description = "Create a new issue/task in the project",
                    parameters = new {
                        type = "object",
                        properties = new {
                            projectId = new { type = "string", description = "The Project ID" },
                            title = new { type = "string", description = "Issue title" },
                            description = new { type = "string", description = "Issue description" },
                            priority = new { type = "integer", description = "Priority (0:Low, 1:Medium, 2:High)" }
                        },
                        required = new[] { "projectId", "title" }
                    }
                }
            },
            new {
                type = "function",
                function = new {
                    name = "update_status",
                    description = "Update issue status (move card)",
                    parameters = new {
                        type = "object",
                        properties = new {
                            issueNumber = new { type = "integer" },
                            projectId = new { type = "string" },
                            newStatus = new { type = "string" }
                        },
                        required = new[] { "issueNumber", "projectId", "newStatus" }
                    }
                }
            },
             new {
                type = "function",
                function = new {
                    name = "get_project_summary",
                    description = "Get a summary of all issues in the project to understand current state",
                    parameters = new {
                        type = "object",
                        properties = new {
                            projectId = new { type = "string" },
                            status = new { type = "string", description = "Optional status filter" }
                        },
                        required = new[] { "projectId" }
                    }
                }
            }
        };

        // 4. Llamar a OpenAI
        var requestBody = new
        {
            model = _model,
            messages = messages,
            tools = tools,
            tool_choice = "auto"
        };

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
        var responseString = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            // Fallback error handling
            return $"Error from OpenAI: {response.StatusCode}";
        }

        using var doc = JsonDocument.Parse(responseString);
        var choice = doc.RootElement.GetProperty("choices")[0];
        var responseMessage = choice.GetProperty("message");

        // 5. Manejar respuesta (Tool Call o Texto)
        if (responseMessage.TryGetProperty("tool_calls", out var toolCalls))
        {
            // La IA quiere ejecutar una función
            var toolCall = toolCalls[0];
            var functionName = toolCall.GetProperty("function").GetProperty("name").GetString();
            var arguments = toolCall.GetProperty("function").GetProperty("arguments").GetString();
            var argsDoc = JsonDocument.Parse(arguments!);

            string toolResult = "";

            if (functionName == "create_issue")
            {
                var pid = Guid.Parse(argsDoc.RootElement.GetProperty("projectId").GetString()!);
                var title = argsDoc.RootElement.GetProperty("title").GetString()!;
                var desc = argsDoc.RootElement.TryGetProperty("description", out var d) ? d.GetString() : null;
                var prio = argsDoc.RootElement.TryGetProperty("priority", out var p) ? p.GetInt32() : (int?)null;

                var issue = await _boardTools.CreateIssueAsync(pid, title, desc, prio);
                toolResult = $"Issue created: #{issue.Number} {issue.Title}";
            }
            else if (functionName == "update_status")
            {
                 var num = argsDoc.RootElement.GetProperty("issueNumber").GetInt32();
                 var pid = Guid.Parse(argsDoc.RootElement.GetProperty("projectId").GetString()!);
                 var status = argsDoc.RootElement.GetProperty("newStatus").GetString()!;
                 
                 var issue = await _boardTools.UpdateIssueStatusAsync(num, pid, status);
                 toolResult = issue != null ? $"Issue updated to {status}" : "Issue not found";
            }
             else if (functionName == "get_project_summary")
            {
                 var pid = Guid.Parse(argsDoc.RootElement.GetProperty("projectId").GetString()!);
                 var status = argsDoc.RootElement.TryGetProperty("status", out var s) ? s.GetString() : null;
                 
                 var summary = await _boardTools.GetProjectIssuesSummaryAsync(pid, status);
                 toolResult = JsonSerializer.Serialize(summary);
            }

            // Guardar ejecución de herramienta como mensaje 'assistant' (para mantener historial)
            // En una implementación completa, se debería hacer un segundo round-trip a OpenAI con el resultado.
            // Por ahora retornamos el resultado de la acción directamente.
            
            var toolMsg = new AiMessage
            {
                ConversationId = conversationId,
                Role = "assistant",
                Content = JsonDocument.Parse(JsonSerializer.Serialize(new { text = $"Executed {functionName}: {toolResult}" })),
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.AiMessages.Add(toolMsg);
            await _context.SaveChangesAsync();

            return toolResult;
        }
        else
        {
            // Respuesta normal de texto
            var content = responseMessage.GetProperty("content").GetString();
            
            var assistantMsg = new AiMessage
            {
                ConversationId = conversationId,
                Role = "assistant",
                Content = JsonDocument.Parse(JsonSerializer.Serialize(new { text = content })),
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.AiMessages.Add(assistantMsg);
            await _context.SaveChangesAsync();

            return content ?? "";
        }
    }
}
