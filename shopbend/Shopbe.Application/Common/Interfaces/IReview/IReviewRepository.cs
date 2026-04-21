using Shopbe.Domain.Entities.Review;
using Shopbe.Application.Review.Dtos;

namespace Shopbe.Application.Common.Interfaces.IReview;

public interface IReviewRepository
{
    Task<Domain.Entities.Review.Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Domain.Entities.Review.Review?> GetByOrderAndProductForUserAsync(
        Guid orderId,
        Guid productId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Entities.Review.Review>> ListVisibleByProductIdAsync(
        Guid productId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountVisibleByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<(double AverageRating, int Count)> GetRatingSummaryAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewableProductDto>> ListReviewableProductsForUserAsync(
        Guid userId,
        bool onlyNotReviewed = false,
        CancellationToken cancellationToken = default);

    Task AddAsync(Domain.Entities.Review.Review review, CancellationToken cancellationToken = default);

    void Remove(Domain.Entities.Review.Review review);
}


