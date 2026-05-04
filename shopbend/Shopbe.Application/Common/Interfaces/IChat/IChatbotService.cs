using Shopbe.Domain.Entities.Chatbot;

namespace Shopbe.Application.Common.Interfaces.IChat;

public interface IChatbotService
{
    /// <summary>
    /// Generates an assistant reply for the given conversation using existing message history.
    /// Implementations should persist the assistant message (Sender="assistant") if requested.
    /// </summary>
    Task<ChatMessage> GenerateAssistantReplyAsync(Guid conversationId, CancellationToken cancellationToken = default);
}

