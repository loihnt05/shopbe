using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IReview;
using Shopbe.Application.Review.Dtos;
using Shopbe.Domain.Entities.Review;
using Shopbe.Domain.Enums;
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

    public async Task<IReadOnlyList<ReviewableProductDto>> ListReviewableProductsForUserAsync(
        Guid userId,
        bool onlyNotReviewed = false,
        CancellationToken cancellationToken = default)
    {
        // Reviewable policy: user must have a DELIVERED order containing the product.
        // One review per (UserId, OrderId, ProductId).

        var deliveredOrderIds = context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
            .Select(o => new { o.Id, o.CreatedAt });

        // Base: (OrderId, PurchasedAt, ProductId, Name)
        var purchased =
            from o in deliveredOrderIds
            join oi in context.OrderItems.AsNoTracking() on o.Id equals oi.OrderId
            join pv in context.ProductVariants.AsNoTracking() on oi.ProductVariantId equals pv.Id
            join p in context.Products.AsNoTracking() on pv.ProductId equals p.Id
            select new
            {
                OrderId = o.Id,
                PurchasedAt = o.CreatedAt,
                ProductId = p.Id,
                ProductName = p.Name
            };

        // Image (best effort: primary image else lowest sort order)
        var productImage =
            from img in context.ProductImages.AsNoTracking()
            group img by img.ProductId
            into g
            select new
            {
                ProductId = g.Key,
                ImageUrl = g
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.SortOrder)
                    .Select(x => x.ImageUrl)
                    .FirstOrDefault()
            };

        var query =
            from x in purchased
            join img in productImage on x.ProductId equals img.ProductId into imgJoin
            from img in imgJoin.DefaultIfEmpty()
            join r in context.Reviews.AsNoTracking().Where(r => r.UserId == userId)
                on new { x.OrderId, x.ProductId } equals new { r.OrderId, r.ProductId } into rJoin
            from r in rJoin.DefaultIfEmpty()
            select new ReviewableProductDto(
                x.OrderId,
                x.ProductId,
                x.ProductName,
                img.ImageUrl,
                x.PurchasedAt,
                r != null,
                r != null ? r.Id : null);

        if (onlyNotReviewed)
            query = query.Where(x => !x.IsReviewed);

        // Deduplicate if same product appears multiple times in same order (multiple variants/items)
        // Keep deterministic ordering: newest purchases first.
        var result = await query
            .OrderByDescending(x => x.PurchasedAt)
            .ThenBy(x => x.ProductName)
            .ToListAsync(cancellationToken);

        return result
            .GroupBy(x => new { x.OrderId, x.ProductId })
            .Select(g => g.First())
            .ToList();
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


