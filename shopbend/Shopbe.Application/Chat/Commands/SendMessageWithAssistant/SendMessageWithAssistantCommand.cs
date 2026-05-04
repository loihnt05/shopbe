using MediatR;
using Shopbe.Application.Chat.Dtos;

namespace Shopbe.Application.Chat.Commands.SendMessageWithAssistant;

public sealed record SendMessageWithAssistantCommand(Guid UserId, Guid ConversationId, SendMessageRequestDto Request)
    : IRequest<IReadOnlyList<ChatMessageDto>>;

