using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Domain.Enums;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Services;

public sealed class RecommendationService(ShopDbContext db, ICacheService cache) : IRecommendationService
{
    public async Task<List<ProductResponseDto>> GetTopSellingAsync(int count = 10)
    {
        count = NormalizeCount(count, 10);
        var cacheKey = $"recommendations:top-selling:v1:count={count}";

        var cached = await cache.GetAsync<List<ProductResponseDto>>(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        var topProductIds = await db.OrderItems
            .AsNoTracking()
            .GroupBy(oi => oi.ProductVariantId)
            .Select(g => new { ProductVariantId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .Join(
                db.ProductVariants.AsNoTracking(),
                x => x.ProductVariantId,
                pv => pv.Id,
                (x, pv) => new { pv.ProductId, x.Qty }
            )
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Qty) })
            .OrderByDescending(x => x.Qty)
            .Take(count)
            .Select(x => x.ProductId)
            .ToListAsync();

        if (topProductIds.Count == 0)
            return [];

        var products = await db.Products
            .AsNoTracking()
            .Where(p => topProductIds.Contains(p.Id))
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsSplitQuery()
            .ToListAsync();

        var byId = products.ToDictionary(p => p.Id);
        var result = topProductIds
            .Where(id => byId.ContainsKey(id))
            .Select(id => ProductDtoMapper.ToResponse(byId[id]))
            .ToList();

        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<List<ProductResponseDto>> GetSimilarProductsAsync(Guid productId, int count = 8)
    {
        count = NormalizeCount(count, 8);
        var cacheKey = $"recommendations:similar:v1:product={productId}:count={count}";

        var cached = await cache.GetAsync<List<ProductResponseDto>>(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.CategoryId, p.BasePrice })
            .FirstOrDefaultAsync();

        if (product is null)
            return [];

        // Same category, similar price range (+/- 25%)
        var minPrice = product.BasePrice * 0.75m;
        var maxPrice = product.BasePrice * 1.25m;

        var products = await db.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
            .OrderBy(p => Math.Abs((double)(p.BasePrice - product.BasePrice)))
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Take(count)
            .AsSplitQuery()
            .ToListAsync();

        var result = products.Select(p => ProductDtoMapper.ToResponse(p, "Similar category & price")).ToList();
        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<List<ProductResponseDto>> GetPersonalizedAsync(Guid userId, int count = 10)
    {
        count = NormalizeCount(count, 10);

        var userGuid = userId;

        // Get top categories based on behavior weight
        var topCategoryIds = await db.UserBehaviors
            .AsNoTracking()
            .Where(b => b.UserId == userGuid && b.CategoryId != null)
            .OrderByDescending(b => b.OccurredAt)
            .Take(200)
            .GroupBy(b => b.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Score = g.Sum(x => x.BehaviorType == BehaviorType.Purchase ? 10
                    : x.BehaviorType == BehaviorType.AddToCart ? 5
                    : 1)
            })
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => x.CategoryId!.Value)
            .ToListAsync();

        if (topCategoryIds.Count == 0)
            return await GetTopSellingAsync(count);

        // Get products from these categories, excluding ones already purchased
        var purchasedProductIds = await db.UserBehaviors
            .AsNoTracking()
            .Where(b => b.UserId == userGuid && b.BehaviorType == BehaviorType.Purchase && b.ProductId != null)
            .Select(b => b.ProductId!.Value)
            .Distinct()
            .ToListAsync();

        var products = await db.Products
            .AsNoTracking()
            .Where(p => topCategoryIds.Contains(p.CategoryId) && !purchasedProductIds.Contains(p.Id))
            .OrderByDescending(p => p.SoldCount)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Take(count)
            .AsSplitQuery()
            .ToListAsync();

        if (products.Count == 0)
            return await GetTopSellingAsync(count);

        return products.Select(p => ProductDtoMapper.ToResponse(p, $"Recommended based on your interest in {p.Category?.Name ?? "similar items"}")).ToList();
    }

    public async Task<List<ProductResponseDto>> GetRandomDiscoverAsync(int limit, List<Guid> excludeIds)
    {
        limit = NormalizeCount(limit, 10);

        var query = db.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (excludeIds != null && excludeIds.Count != 0)
        {
            query = query.Where(p => !excludeIds.Contains(p.Id));
        }

        var productIds = await query
            .OrderBy(p => EF.Functions.Random())
            .Select(p => p.Id)
            .Take(limit)
            .ToListAsync();

        if (productIds.Count == 0)
        {
            return [];
        }

        var products = await db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Variants)
            .AsSplitQuery()
            .ToListAsync();

        var byId = products.ToDictionary(p => p.Id);
        return productIds
            .Where(id => byId.ContainsKey(id))
            .Select(id => ProductDtoMapper.ToResponse(byId[id], "Discover something new"))
            .ToList();
    }

    public async Task<List<ProductResponseDto>> GetFrequentlyBoughtTogetherAsync(Guid productId, int count = 5)
    {
        count = NormalizeCount(count, 5);
        var cacheKey = $"recommendations:frequently-bought-together:v1:product={productId}:count={count}";

        var cached = await cache.GetAsync<List<ProductResponseDto>>(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        // Find variants for this product
        var variantIds = await db.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == productId)
            .Select(v => v.Id)
            .ToListAsync();

        // Find orders containing these variants
        var orderIds = await db.OrderItems
            .AsNoTracking()
            .Where(oi => variantIds.Contains(oi.ProductVariantId))
            .Select(oi => oi.OrderId)
            .Distinct()
            .ToListAsync();

        if (orderIds.Count == 0) return [];

        // Find other products in those orders
        var otherProductIds = await db.OrderItems
            .AsNoTracking()
            .Where(oi => orderIds.Contains(oi.OrderId) && !variantIds.Contains(oi.ProductVariantId))
            .GroupBy(oi => oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, Count = g.Count() })
            .Join(db.ProductVariants.AsNoTracking(), x => x.VariantId, v => v.Id, (x, v) => new { v.ProductId, x.Count })
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Sum(x => x.Count) })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .Select(x => x.ProductId)
            .ToListAsync();

        var products = await db.Products
            .AsNoTracking()
            .Where(p => otherProductIds.Contains(p.Id))
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsSplitQuery()
            .ToListAsync();

        var result = products.Select(p => ProductDtoMapper.ToResponse(p, "Frequently bought together")).ToList();
        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<List<ProductResponseDto>> GetRecentlyViewedAsync(Guid userId, int count = 10)
    {
        count = NormalizeCount(count, 10);

        var productIds = await db.UserBehaviors
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.BehaviorType == BehaviorType.ProductView && b.ProductId != null)
            .GroupBy(b => b.ProductId!.Value)
            .Select(g => new { ProductId = g.Key, LastViewedAt = g.Max(x => x.OccurredAt) })
            .OrderByDescending(x => x.LastViewedAt)
            .Take(count)
            .Select(x => x.ProductId)
            .ToListAsync();

        var products = await db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsSplitQuery()
            .ToListAsync();

        var byId = products.ToDictionary(p => p.Id);
        
        // Return in the order of viewing
        return productIds
            .Where(id => byId.ContainsKey(id))
            .Select(id => ProductDtoMapper.ToResponse(byId[id]))
            .ToList();
    }

    private static int NormalizeCount(int count, int @default)
    {
        if (count <= 0) return @default;
        return Math.Clamp(count, 1, 50);
    }
}
