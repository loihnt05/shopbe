using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IChat;
using Shopbe.Domain.Entities.Chatbot;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ChatRepositories;

public sealed class ConversationRepository(ShopDbContext context) : IConversationRepository
{
    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Conversation?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Conversations
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "active", cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Conversations
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await context.Conversations.AddAsync(conversation, cancellationToken);
    }
}

