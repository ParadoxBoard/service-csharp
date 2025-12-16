using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
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

    private const int MaxHistoryMessages = 20;       // límite de mensajes en contexto
    private const int MaxTextLength = 2000;          // recorte básico para no desbordar tokens

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

        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
            ?? throw new InvalidOperationException("OPENAI_API_KEY no está configurado");
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

    public async Task<IEnumerable<AiConversation>> GetConversationsAsync(Guid userId, Guid? projectId = null)
    {
        var query = _context.AiConversations.AsQueryable()
            .Where(c => c.CreatedBy == userId);

        if (projectId.HasValue)
        {
            query = query.Where(c => c.ProjectId == projectId.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AiMessage>> GetMessagesAsync(Guid conversationId, Guid userId, int take = 50, int skip = 0)
    {
        await EnsureConversationOwnerAsync(conversationId, userId);

        return await _context.AiMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<string> SendMessageAsync(Guid conversationId, string message, Guid userId)
    {
        await EnsureConversationOwnerAsync(conversationId, userId);

        // 1. Guardar mensaje del usuario
        var userMsg = new AiMessage
        {
            ConversationId = conversationId,
            Role = "user",
            Content = JsonDocument.Parse(JsonSerializer.Serialize(new { text = Trim(message) })),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.AiMessages.Add(userMsg);
        await _context.SaveChangesAsync();

        // 2. Preparar contexto para OpenAI (historial reciente)
        var history = await _context.AiMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Take(MaxHistoryMessages)
            .ToListAsync();

        var systemPrompt = new
        {
            role = "system",
            content = "Eres un asistente especializado en metodologías ágiles (Scrum, Kanban). " +
                      "Ayudas a planificar, priorizar, refinar y dar seguimiento a issues. " +
                      "Sé conciso, da pasos claros, propone backlog items, criterios de aceptación y estados. " +
                      "Si usas herramientas, explica brevemente el resultado."
        };

        var messages = new List<object> { systemPrompt };
        messages.AddRange(history.Select(m => new
        {
            role = m.Role,
            content = m.Content.RootElement.TryGetProperty("text", out var t) ? Trim(t.GetString() ?? "") : ""
        }));

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
            },
            new {
                type = "function",
                function = new {
                    name = "comment_on_github_issue",
                    description = "Post a comment on a GitHub issue",
                    parameters = new {
                        type = "object",
                        properties = new {
                            repoId = new { type = "integer", description = "The Repository ID (long)" },
                            issueNumber = new { type = "integer", description = "The Issue Number" },
                            comment = new { type = "string", description = "The comment content" }
                        },
                        required = new[] { "repoId", "issueNumber", "comment" }
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
            else if (functionName == "comment_on_github_issue")
            {
                 var repoId = argsDoc.RootElement.GetProperty("repoId").GetInt64();
                 var issueNum = argsDoc.RootElement.GetProperty("issueNumber").GetInt32();
                 var comment = argsDoc.RootElement.GetProperty("comment").GetString()!;
                 
                 toolResult = await _boardTools.CommentOnGithubIssue(repoId, issueNum, comment);
            }

            // Guardar resultado de herramienta
            var toolMsg = new AiMessage
            {
                ConversationId = conversationId,
                Role = "assistant",
                Content = JsonDocument.Parse(JsonSerializer.Serialize(new { text = $"Executed {functionName}: {toolResult}" })),
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.AiMessages.Add(toolMsg);
            await _context.SaveChangesAsync();

            // Segundo round-trip para que la IA genere respuesta final con el resultado de la herramienta
            var followupMessages = new List<object>(messages)
            {
                new
                {
                    role = "assistant",
                    content = $"Executed {functionName}: {toolResult}"
                }
            };

            var followupBody = new
            {
                model = _model,
                messages = followupMessages,
                tool_choice = "none"
            };

            var followupResponse = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", followupBody);
            var followupString = await followupResponse.Content.ReadAsStringAsync();

            if (!followupResponse.IsSuccessStatusCode)
            {
                return toolResult; // devolvemos el resultado de la herramienta si la segunda llamada falla
            }

            using var followDoc = JsonDocument.Parse(followupString);
            var followChoice = followDoc.RootElement.GetProperty("choices")[0];
            var followMsg = followChoice.GetProperty("message");
            var finalContent = followMsg.GetProperty("content").GetString();

            var assistantMsgFinal = new AiMessage
            {
                ConversationId = conversationId,
                Role = "assistant",
                Content = JsonDocument.Parse(JsonSerializer.Serialize(new { text = finalContent })),
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.AiMessages.Add(assistantMsgFinal);
            await _context.SaveChangesAsync();

            return finalContent ?? toolResult;
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

    private static string Trim(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= MaxTextLength ? text : text[..MaxTextLength];
    }

    private async Task EnsureConversationOwnerAsync(Guid conversationId, Guid userId)
    {
        var convo = await _context.AiConversations.FirstOrDefaultAsync(c => c.Id == conversationId);
        if (convo == null)
        {
            throw new InvalidOperationException("Conversación no encontrada");
        }

        if (convo.CreatedBy != userId)
        {
            throw new UnauthorizedAccessException("No tienes acceso a esta conversación");
        }
    }
}
