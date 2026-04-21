using MediatR;
using Shopbe.Application.Common.Interfaces.IReview;
using Shopbe.Application.Review.Dtos;

namespace Shopbe.Application.Review.Queries.GetProductReviews;

public sealed record GetProductReviewsQuery(Guid ProductId, int Skip, int Take) : IRequest<PagedReviewsDto>;

public sealed class GetProductReviewsQueryHandler(IReviewRepository reviews) : IRequestHandler<GetProductReviewsQuery, PagedReviewsDto>
{
    public async Task<PagedReviewsDto> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var total = await reviews.CountVisibleByProductIdAsync(request.ProductId, cancellationToken);
        var items = await reviews.ListVisibleByProductIdAsync(request.ProductId, request.Skip, request.Take, cancellationToken);
        var (avg, count) = await reviews.GetRatingSummaryAsync(request.ProductId, cancellationToken);

        var summary = new ReviewSummaryDto(request.ProductId, avg, count);
        return new PagedReviewsDto(
            request.ProductId,
            total,
            Math.Max(0, request.Skip),
            request.Take <= 0 ? 20 : Math.Min(100, request.Take),
            summary,
            items.Select(ReviewDtoMapper.ToDto).ToList());
    }
}

