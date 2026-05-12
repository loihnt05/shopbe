using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Order.Commands.CancelOrder;
using Shopbe.Application.Order.Commands.CreateOrder;
using Shopbe.Application.Order.Dtos;
using Shopbe.Application.Order.Queries.GetMyOrderById;
using Shopbe.Application.Order.Queries.GetMyOrderHistory;
using Shopbe.Application.Order.Queries.GetMyOrders;
using Shopbe.Application.Review.Dtos;
using Shopbe.Application.Review.Queries.GetMyReviewableProducts;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IMediator mediator, ICurrentUser currentUser, IUnitOfWork unitOfWork) : ControllerBase
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

    [HttpGet]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new GetMyOrdersQuery(userId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new GetMyOrderByIdQuery(userId, id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new GetMyOrderHistoryQuery(userId, id), cancellationToken);
        return Ok(result);
    }

    // Purchase history (products): list of products the current user bought in DELIVERED orders.
    // Reuses the same policy/data as reviewable products, but exposed under /api/orders for discoverability.
    [HttpGet("me/purchased-products")]
    public async Task<ActionResult<IReadOnlyList<ReviewableProductDto>>> GetMyPurchasedProducts(
        [FromQuery] bool onlyNotReviewed = false,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMyReviewableProductsQuery(onlyNotReviewed), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var created = await mediator.Send(new CreateOrderCommand(userId, request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    public sealed class CancelOrderRequestDto
    {
        public string? Reason { get; set; }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequestDto request, CancellationToken cancellationToken)
    {
        var userId = await GetAppUserIdAsync(cancellationToken);
        var result = await mediator.Send(new CancelOrderCommand(userId, id, request.Reason), cancellationToken);
        return Ok(result);
    }
}

