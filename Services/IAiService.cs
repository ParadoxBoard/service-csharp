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

    /// <summary>
    /// Obtiene las conversaciones del usuario (opcionalmente filtradas por proyecto).
    /// </summary>
    Task<IEnumerable<AiConversation>> GetConversationsAsync(Guid userId, Guid? projectId = null);

    /// <summary>
    /// Obtiene los mensajes de una conversación, validando propiedad del usuario.
    /// </summary>
    Task<IEnumerable<AiMessage>> GetMessagesAsync(Guid conversationId, Guid userId, int take = 50, int skip = 0);
}
