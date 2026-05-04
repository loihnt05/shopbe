using MediatR;
using Shopbe.Application.Chat.Dtos;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Entities.Chatbot;

namespace Shopbe.Application.Chat.Commands.CreateConversation;

public sealed class CreateConversationHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        // Keep one active conversation per user as a sensible default.
        var conversation = await unitOfWork.Conversations.GetActiveByUserIdAsync(request.UserId, cancellationToken);

        if (conversation is null)
        {
            conversation = new Conversation
            {
                UserId = request.UserId,
                Status = "active",
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await unitOfWork.Conversations.AddAsync(conversation, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var last = await unitOfWork.ChatMessages.GetLastMessageAsync(conversation.Id, cancellationToken);
        var lastMessageAt = last?.CreatedAt ?? conversation.StartedAt;

        return new ConversationDto(conversation.Id, conversation.Status, conversation.StartedAt, conversation.EndedAt, lastMessageAt, null);
    }
}


