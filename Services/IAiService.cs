using service_csharp.Models;

namespace service_csharp.Services;

public interface IAiService
{
    /// <summary>
    /// Envía un mensaje a la IA dentro de una conversación.
    /// La IA puede decidir ejecutar herramientas (BoardTools) basándose en este mensaje.
    /// </summary>
    Task<string> SendMessageAsync(Guid conversationId, string message, Guid userId);

    /// <summary>
    /// Inicia una nueva conversación con contexto opcional de un proyecto.
    /// </summary>
    Task<AiConversation> CreateConversationAsync(Guid? projectId, Guid userId, string? title = null);
}
