using MediatR;
using Shopbe.Application.Chat.Dtos;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IChat;
using Shopbe.Domain.Entities.Chatbot;

namespace Shopbe.Application.Chat.Commands.SendMessageWithAssistant;

public sealed class SendMessageWithAssistantHandler(IUnitOfWork unitOfWork, IChatbotService chatbotService)
    : IRequestHandler<SendMessageWithAssistantCommand, IReadOnlyList<ChatMessageDto>>
{
    public async Task<IReadOnlyList<ChatMessageDto>> Handle(SendMessageWithAssistantCommand request, CancellationToken cancellationToken)
    {
        var conversation = await unitOfWork.Conversations.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null || conversation.UserId != request.UserId)
            throw new UnauthorizedAccessException("Conversation not found");

        if (string.IsNullOrWhiteSpace(request.Request.Content))
            throw new ArgumentException("Content is required", nameof(request.Request.Content));

        var now = DateTime.UtcNow;
        var userMessage = new ChatMessage
        {
            ConversationId = request.ConversationId,
            Sender = "user",
            Content = request.Request.Content.Trim(),
            Metadata = request.Request.Metadata,
            CreatedAt = now,
            UpdatedAt = now
        };

        await unitOfWork.ChatMessages.AddAsync(userMessage, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Flow A: generate assistant reply immediately.
        var assistant = await chatbotService.GenerateAssistantReplyAsync(request.ConversationId, cancellationToken);

        return new List<ChatMessageDto>
        {
            new(userMessage.Id, userMessage.ConversationId, userMessage.Sender, userMessage.Content, userMessage.Metadata, userMessage.CreatedAt),
            new(assistant.Id, assistant.ConversationId, assistant.Sender, assistant.Content, assistant.Metadata, assistant.CreatedAt)
        };
    }
}

