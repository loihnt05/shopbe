using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Product.Products.Commands.CreateProduct;
using Shopbe.Application.Product.Products.Commands.DeleteProduct;
using Shopbe.Application.Product.Products.Commands.UpdateProduct;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Application.Product.Products.Queries.GetAllProducts;
using Shopbe.Application.Product.Products.Queries.GetProductById;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Enums;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IBehaviorTrackingService _behaviorTracking;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ProductController(
        IMediator mediator,
        IBehaviorTrackingService behaviorTracking,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _behaviorTracking = behaviorTracking;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    private async Task<Guid?> TryGetAppUserIdAsync()
    {
        // Anonymous browsing is allowed; we only attach userId when authenticated.
        var keycloakId = _currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            return null;

        var user = await _unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        return user?.Id;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryDto filter, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllProductsQuery(filter), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (result is null)
            return NotFound();

        // Track product view for recommendations.
        // Best effort: never fail the request if tracking fails.
        try
        {
            var appUserId = await TryGetAppUserIdAsync();

            await _behaviorTracking.TrackAsync(
                userId: appUserId,
                sessionId: null,
                correlationId: HttpContext.TraceIdentifier,
                behaviorType: BehaviorType.ProductView,
                actionType: "ProductView",
                productId: id,
                categoryId: result.CategoryId,
                source: "web",
                device: Request.Headers.UserAgent.ToString(),
                referrer: Request.Headers.Referer.ToString(),
                userAgent: Request.Headers.UserAgent.ToString(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                occurredAt: DateTime.UtcNow,
                ct: cancellationToken);
        }
        catch
        {
            // ignored
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] ProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateProductCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProductCommand(id, request), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
} 