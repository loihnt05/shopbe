using Shopbe.Domain.Entities.Review;

namespace Shopbe.Application.Review.Dtos;

public sealed record ReviewImageDto(Guid Id, string ImageUrl, int SortOrder);

public sealed record ReviewDto(
    Guid Id,
    Guid UserId,
    Guid ProductId,
    Guid OrderId,
    short Rating,
    string? Comment,
    bool IsVisible,
    DateTime CreatedAt,
    IReadOnlyList<ReviewImageDto> Images);

public sealed record ReviewSummaryDto(Guid ProductId, double AverageRating, int Count);

public sealed record PagedReviewsDto(
    Guid ProductId,
    int Total,
    int Skip,
    int Take,
    ReviewSummaryDto Summary,
    IReadOnlyList<ReviewDto> Items);

public sealed class CreateReviewRequestDto
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public short Rating { get; set; }
    public string? Comment { get; set; }
    public List<string>? ImageUrls { get; set; }
}

public static class ReviewDtoMapper
{
    public static ReviewDto ToDto(Shopbe.Domain.Entities.Review.Review r)
        => new(
            r.Id,
            r.UserId,
            r.ProductId,
            r.OrderId,
            r.Rating,
            r.Comment,
            r.IsVisible,
            r.CreatedAt,
            r.ReviewImages
                .OrderBy(i => i.SortOrder)
                .Select(i => new ReviewImageDto(i.Id, i.ImageUrl, i.SortOrder))
                .ToList());
}

