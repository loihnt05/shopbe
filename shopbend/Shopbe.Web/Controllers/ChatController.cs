using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Chat.Commands.CreateConversation;
using Shopbe.Application.Chat.Commands.SendMessage;
using Shopbe.Application.Chat.Dtos;
using Shopbe.Application.Chat.Queries.GetConversationMessages;
using Shopbe.Application.Chat.Queries.GetMyConversations;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public sealed class ChatController(
    IMediator mediator,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ControllerBase
{
    private async Task<Guid> GetAppUserIdAsync(CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new UnauthorizedAccessException("Missing user identity");

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        return user.Id;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetMyConversations(CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new GetMyConversationsQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a conversation for the current user (or returns the current active one).
    /// </summary>
    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new CreateConversationCommand(userId, request), cancellationToken);
        return Ok(result);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetMessages(
        Guid conversationId,
        [FromQuery] DateTime? after,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new GetConversationMessagesQuery(userId, conversationId, after, take), cancellationToken);
        return Ok(result);
    }

    [HttpPost("conversations/{conversationId:guid}/messages")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(
        Guid conversationId,
        [FromBody] SendMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new SendMessageCommand(userId, conversationId, request), cancellationToken);
        return Ok(result);
    }
}

