using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services;

public sealed class RecommendationService(ShopDbContext db) : IRecommendationService
{
    public async Task<List<ProductResponseDto>> GetTopSellingAsync(int count = 10)
    {
        count = NormalizeCount(count, 10);

        // Use OrderItems as the source of truth for "selling".
        var topProductIds = await db.OrderItems
            .AsNoTracking()
            .GroupBy(oi => oi.ProductVariantId)
            .Select(g => new { ProductVariantId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(count)
            .Join(
                db.ProductVariants.AsNoTracking(),
                x => x.ProductVariantId,
                pv => pv.Id,
                (x, pv) => pv.ProductId
            )
            .Distinct()
            .ToListAsync();

        if (topProductIds.Count == 0)
            return [];

        // Preserve ranking.
        var products = await db.Products
            .AsNoTracking()
            .Where(p => topProductIds.Contains(p.Id))
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .ToListAsync();

        var byId = products.ToDictionary(p => p.Id);
        return topProductIds
            .Where(id => byId.ContainsKey(id))
            .Select(id => ProductDtoMapper.ToResponse(byId[id]))
            .ToList();
    }

    public async Task<List<ProductResponseDto>> GetSimilarProductsAsync(Guid productId, int count = 8)
    {
        count = NormalizeCount(count, 8);

        var productGuid = productId;
        var categoryId = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productGuid)
            .Select(p => (Guid?)p.CategoryId)
            .FirstOrDefaultAsync();

        if (categoryId is null)
            return [];

        var products = await db.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.Id != productGuid)
            .OrderByDescending(p => p.CreatedAt)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Take(count)
            .ToListAsync();

        return products.Select(ProductDtoMapper.ToResponse).ToList();
    }

    public async Task<List<ProductResponseDto>> GetPersonalizedAsync(Guid userId, int count = 10)
    {
        count = NormalizeCount(count, 10);

        var userGuid = userId;

        // Strategy:
        // 1) Find most interacted category based on recent events.
        // 2) Recommend newest items in that category.
        // 3) Fallback to top selling.
        var topCategoryId = await db.UserBehaviors
            .AsNoTracking()
            .Where(b => b.UserId == userGuid && b.CategoryId != null)
            .OrderByDescending(b => b.OccurredAt)
            .Take(200)
            .GroupBy(b => b.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Score = g.Sum(x => x.BehaviorType == BehaviorType.Purchase ? 5
                    : x.BehaviorType == BehaviorType.AddToCart ? 3
                    : 1)
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.CategoryId)
            .FirstOrDefaultAsync();

        if (topCategoryId is null)
            return await GetTopSellingAsync(count);

        var products = await db.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == topCategoryId)
            .OrderByDescending(p => p.CreatedAt)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Take(count)
            .ToListAsync();

        return products.Count != 0
            ? products.Select(ProductDtoMapper.ToResponse).ToList()
            : await GetTopSellingAsync(count);
    }

    private static int NormalizeCount(int count, int @default)
    {
        if (count <= 0) return @default;
        return Math.Clamp(count, 1, 50);
    }

}




