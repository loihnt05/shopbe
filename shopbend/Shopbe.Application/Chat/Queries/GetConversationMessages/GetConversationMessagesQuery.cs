using MediatR;
using Shopbe.Application.Chat.Dtos;

namespace Shopbe.Application.Chat.Queries.GetConversationMessages;

public sealed record GetConversationMessagesQuery(
    Guid UserId,
    Guid ConversationId,
    DateTime? After,
    int Take
) : IRequest<IReadOnlyList<ChatMessageDto>>;

