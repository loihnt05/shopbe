using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Wishlist.Commands.AddWishlistItem;
using Shopbe.Application.Wishlist.Commands.RemoveWishlistItem;
using Shopbe.Application.Wishlist.Dtos;
using Shopbe.Application.Wishlist.Queries.GetMyWishlist;
using Shopbe.Application.Wishlist.Queries.IsInWishlist;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
public sealed class WishlistController(IMediator mediator, ICurrentUser currentUser, IUnitOfWork unitOfWork) : ControllerBase
{
    private async Task<Guid> GetAppUserIdAsync()
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new UnauthorizedAccessException("Missing user identity");

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        return user.Id;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWishlist(CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync();
        var result = await mediator.Send(new GetMyWishlistQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("contains/{productId:guid}")]
    public async Task<IActionResult> Contains(Guid productId, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync();
        var result = await mediator.Send(new IsInWishlistQuery(userId, productId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddWishlistItemRequestDto request, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync();
        var result = await mediator.Send(new AddWishlistItemCommand(userId, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync();
        var result = await mediator.Send(new RemoveWishlistItemCommand(userId, productId), cancellationToken);
        return Ok(result);
    }
}
