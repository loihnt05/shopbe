using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Cart.Commands.AddItem;
using Shopbe.Application.Cart.Commands.ClearCart;
using Shopbe.Application.Cart.Commands.RemoveItem;
using Shopbe.Application.Cart.Commands.SetItemQuantity;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Cart.Queries.GetMyCart;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(IMediator mediator, ICurrentUser currentUser, IUnitOfWork unitOfWork) : ControllerBase
{
    private async Task<Guid> GetAppUserIdAsync(CancellationToken cancellationToken)
    {
        // The app uses Keycloak subject for identity; Users table stores Guid Id.
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new UnauthorizedAccessException("Missing user identity");

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        return user.Id;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart(CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new GetMyCartQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequestDto request, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new AddItemCommand(userId, request), cancellationToken);
        return Ok(result);
    }

    [HttpPut("items/{productVariantId:guid}")]
    public async Task<IActionResult> SetItemQuantity(Guid productVariantId, [FromBody] SetCartItemQuantityRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new SetItemQuantityCommand(userId, productVariantId, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("items/{productVariantId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productVariantId, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new RemoveItemCommand(userId, productVariantId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new ClearCartCommand(userId), cancellationToken);
        return Ok(result);
    }
}

