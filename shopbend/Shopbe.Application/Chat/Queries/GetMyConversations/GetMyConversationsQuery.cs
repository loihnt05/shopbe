using MediatR;
using Shopbe.Application.Chat.Dtos;

namespace Shopbe.Application.Chat.Queries.GetMyConversations;

public sealed record GetMyConversationsQuery(Guid UserId) : IRequest<IReadOnlyList<ConversationDto>>;

