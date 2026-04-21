using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IReview;
using Shopbe.Domain.Entities.Review;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ReviewRepositories;

public class ReviewRepository(ShopDbContext context) : IReviewRepository
{
    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Reviews
            .Include(r => r.ReviewImages)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Review?> GetByOrderAndProductForUserAsync(Guid orderId, Guid productId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Reviews
            .Include(r => r.ReviewImages)
            .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ProductId == productId && r.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> ListVisibleByProductIdAsync(Guid productId, int skip, int take,
        CancellationToken cancellationToken = default)
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 20;
        if (take > 100) take = 100;

        return await context.Reviews
            .AsNoTracking()
            .Include(r => r.ReviewImages)
            .Where(r => r.ProductId == productId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountVisibleByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await context.Reviews
            .AsNoTracking()
            .CountAsync(r => r.ProductId == productId && r.IsVisible, cancellationToken);
    }

    public async Task<(double AverageRating, int Count)> GetRatingSummaryAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsVisible);

        var count = await query.CountAsync(cancellationToken);
        if (count == 0) return (0, 0);

        var avg = await query.AverageAsync(r => (double)r.Rating, cancellationToken);
        return (avg, count);
    }

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        await context.Reviews.AddAsync(review, cancellationToken);
    }

    public void Remove(Review review)
    {
        context.Reviews.Remove(review);
    }
}

