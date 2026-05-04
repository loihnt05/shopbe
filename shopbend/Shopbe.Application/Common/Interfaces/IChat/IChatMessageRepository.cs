using Shopbe.Domain.Entities.Chatbot;

namespace Shopbe.Application.Common.Interfaces.IChat;

public interface IChatMessageRepository
{
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid conversationId, DateTime? after, int take,
        CancellationToken cancellationToken = default);

    Task<ChatMessage?> GetLastMessageAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default);
}

