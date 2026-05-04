using MediatR;
using Shopbe.Application.Chat.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Chat.Queries.GetMyConversations;

public sealed class GetMyConversationsHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMyConversationsQuery, IReadOnlyList<ConversationDto>>
{
    public async Task<IReadOnlyList<ConversationDto>> Handle(GetMyConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await unitOfWork.Conversations.GetByUserIdAsync(request.UserId, cancellationToken);

        var dtos = new List<ConversationDto>(conversations.Count);
        foreach (var c in conversations)
        {
            var last = await unitOfWork.ChatMessages.GetLastMessageAsync(c.Id, cancellationToken);
            dtos.Add(new ConversationDto(
                c.Id,
                c.Status,
                c.StartedAt,
                c.EndedAt,
                last?.CreatedAt ?? c.StartedAt,
                last?.Content is null
                    ? null
                    : (last.Content.Length <= 120 ? last.Content : last.Content[..120])
            ));
        }

        return dtos
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();
    }
}


