using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IChat;
using Shopbe.Domain.Entities.Chatbot;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ChatRepositories;

public sealed class ChatMessageRepository(ShopDbContext context) : IChatMessageRepository
{
    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid conversationId, DateTime? after, int take,
        CancellationToken cancellationToken = default)
    {
        var safeTake = take is <= 0 or > 200 ? 50 : take;

        var query = context.ChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        if (after.HasValue)
            query = query.Where(m => m.CreatedAt > after.Value);

        return await query
            .OrderBy(m => m.CreatedAt)
            .Take(safeTake)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatMessage?> GetLastMessageAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await context.ChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        await context.ChatMessages.AddAsync(message, cancellationToken);
    }
}

