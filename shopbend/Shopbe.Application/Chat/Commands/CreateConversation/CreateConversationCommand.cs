using MediatR;
using Shopbe.Application.Chat.Dtos;

namespace Shopbe.Application.Chat.Commands.CreateConversation;

public sealed record CreateConversationCommand(Guid UserId, CreateConversationRequestDto Request) : IRequest<ConversationDto>;

