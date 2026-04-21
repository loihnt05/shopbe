using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Review.Commands.CreateReview;
using Shopbe.Application.Review.Dtos;
using Shopbe.Application.Review.Queries.GetMyReviewableProducts;
using Shopbe.Application.Review.Queries.GetProductReviews;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(IMediator mediator) : ControllerBase
{
    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedReviewsDto>> GetByProduct(Guid productId, [FromQuery] int skip = 0,
        [FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetProductReviewsQuery(productId, skip, take), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateReviewCommand(request), cancellationToken);
        return Ok(result);
    }

    [HttpGet("me/reviewable-products")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ReviewableProductDto>>> GetMyReviewableProducts(
        [FromQuery] bool onlyNotReviewed = false,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMyReviewableProductsQuery(onlyNotReviewed), cancellationToken);
        return Ok(result);
    }
}

