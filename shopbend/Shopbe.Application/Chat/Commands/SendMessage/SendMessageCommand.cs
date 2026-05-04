using MediatR;
using Shopbe.Application.Chat.Dtos;

namespace Shopbe.Application.Chat.Commands.SendMessage;

public sealed record SendMessageCommand(Guid UserId, Guid ConversationId, SendMessageRequestDto Request) : IRequest<ChatMessageDto>;

