using MediatR;
using Shopbe.Application.Chat.Dtos;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Entities.Chatbot;

namespace Shopbe.Application.Chat.Commands.SendMessage;

public sealed class SendMessageHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SendMessageCommand, ChatMessageDto>
{
    public async Task<ChatMessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await unitOfWork.Conversations.GetByIdAsync(request.ConversationId, cancellationToken);

        if (conversation is null || conversation.UserId != request.UserId)
            throw new UnauthorizedAccessException("Conversation not found");

        if (string.IsNullOrWhiteSpace(request.Request.Content))
            throw new ArgumentException("Content is required", nameof(request.Request.Content));

        var now = DateTime.UtcNow;
        var message = new ChatMessage
        {
            ConversationId = request.ConversationId,
            Sender = "user",
            Content = request.Request.Content.Trim(),
            Metadata = request.Request.Metadata,
            CreatedAt = now,
            UpdatedAt = now
        };

        await unitOfWork.ChatMessages.AddAsync(message, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ChatMessageDto(message.Id, message.ConversationId, message.Sender, message.Content, message.Metadata, message.CreatedAt);
    }
}


