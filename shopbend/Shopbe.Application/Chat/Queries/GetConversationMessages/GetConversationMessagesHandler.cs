using MediatR;
using Shopbe.Application.Chat.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Chat.Queries.GetConversationMessages;

public sealed class GetConversationMessagesHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetConversationMessagesQuery, IReadOnlyList<ChatMessageDto>>
{
    public async Task<IReadOnlyList<ChatMessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await unitOfWork.Conversations.GetByIdAsync(request.ConversationId, cancellationToken);

        if (conversation is null || conversation.UserId != request.UserId)
            throw new UnauthorizedAccessException("Conversation not found");

        var take = request.Take is <= 0 or > 200 ? 50 : request.Take;

        var messages = await unitOfWork.ChatMessages.GetMessagesAsync(request.ConversationId, request.After, take, cancellationToken);
        return messages
            .Select(m => new ChatMessageDto(m.Id, m.ConversationId, m.Sender, m.Content, m.Metadata, m.CreatedAt))
            .ToList();
    }
}


