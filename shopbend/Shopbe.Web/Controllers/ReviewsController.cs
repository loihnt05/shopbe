using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Review.Commands.CreateReview;
using Shopbe.Application.Review.Dtos;
using Shopbe.Application.Review.Queries.GetMyReviewableProducts;
using Shopbe.Application.Review.Queries.GetProductReviews;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(IMediator mediator) : ControllerBase
{
    private const int MaxUploadImages = 6;
    private const long MaxUploadBytes = 5 * 1024 * 1024; // 5MB each

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

    [HttpPost("images")]
    [Authorize]
    [RequestSizeLimit(MaxUploadImages * MaxUploadBytes)]
    public async Task<ActionResult<IReadOnlyList<UploadedImageDto>>> UploadImages(
        [FromServices] IFileStorage fileStorage,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken = default)
    {
        if (files is null || files.Count == 0)
            return BadRequest("No files uploaded.");

        if (files.Count > MaxUploadImages)
            return BadRequest($"Maximum {MaxUploadImages} images allowed.");

        static bool IsAllowedContentType(string? ct)
            => ct is not null && (ct.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)
                                  || ct.Equals("image/png", StringComparison.OrdinalIgnoreCase)
                                  || ct.Equals("image/webp", StringComparison.OrdinalIgnoreCase));

        var result = new List<UploadedImageDto>(files.Count);

        foreach (var file in files)
        {
            if (file.Length <= 0)
                continue;

            if (file.Length > MaxUploadBytes)
                return BadRequest($"File '{file.FileName}' exceeds {MaxUploadBytes} bytes.");

            if (!IsAllowedContentType(file.ContentType))
                return BadRequest($"File '{file.FileName}' has unsupported content type '{file.ContentType}'.");

            await using var stream = file.OpenReadStream();
            var url = await fileStorage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);
            result.Add(new UploadedImageDto(url, file.FileName, file.Length));
        }

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

