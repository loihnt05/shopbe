using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ProductRepositories;

public class ProductRepository(ShopDbContext context) : IProductRepository
{
    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Seller)
                .ThenInclude(s => s!.SellerProfile)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Variants)
                .ThenInclude(v => v.ProductVariantAttributes)
                    .ThenInclude(pva => pva.AttributeValue)
                        .ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Seller)
                .ThenInclude(s => s!.SellerProfile)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Variants)
                .ThenInclude(v => v.ProductVariantAttributes)
                    .ThenInclude(pva => pva.AttributeValue)
                        .ThenInclude(av => av.Attribute)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsBySellerIdAsync(Guid sellerId)
    {
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Seller)
                .ThenInclude(s => s!.SellerProfile)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Variants)
                .ThenInclude(v => v.ProductVariantAttributes)
                    .ThenInclude(pva => pva.AttributeValue)
                        .ThenInclude(av => av.Attribute)
            .Where(p => p.SellerId == sellerId)
            .ToListAsync();
    }

    private IQueryable<Product> BuildSearchQuery(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating)
    {
        var query = context.Products
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{name}%"));
        }

        if (categoryIds != null && categoryIds.Any())
        {
            query = query.Where(p => categoryIds.Contains(p.CategoryId));
        }

        if (categorySlugs != null && categorySlugs.Any())
        {
            query = query.Where(p => p.Category != null && categorySlugs.Contains(p.Category.Slug));
        }

        if (brandIds != null && brandIds.Any())
        {
            query = query.Where(p => p.BrandId != null && brandIds.Contains(p.BrandId.Value));
        }

        if (minBasePrice is not null)
        {
            query = query.Where(p => p.BasePrice >= minBasePrice);
        }

        if (maxBasePrice is not null)
        {
            query = query.Where(p => p.BasePrice <= maxBasePrice);
        }

        if (minRating is not null && minRating > 0)
        {
            query = query.Where(p => p.Reviews.Any() 
                ? p.Reviews.Average(r => (double)r.Rating) >= minRating 
                : 0 >= minRating);
        }

        return query;
    }

    public async Task<IEnumerable<Product>> GetProductsPageAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating,
        string? sortBy,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = BuildSearchQuery(name, categoryIds, categorySlugs, brandIds, minBasePrice, maxBasePrice, minRating)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Seller)
                .ThenInclude(s => s!.SellerProfile)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Variants)
                .ThenInclude(v => v.ProductVariantAttributes)
                    .ThenInclude(pva => pva.AttributeValue)
                        .ThenInclude(av => av.Attribute);

        query = sortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.BasePrice),
            "price_desc" => query.OrderByDescending(p => p.BasePrice),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "popular" => query.OrderByDescending(p => p.SoldCount),
            "rating" => query.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating,
        CancellationToken cancellationToken = default)
    {
        return await BuildSearchQuery(name, categoryIds, categorySlugs, brandIds, minBasePrice, maxBasePrice, minRating)
            .CountAsync(cancellationToken);
    }

    public async Task<IDictionary<Guid, int>> GetCategoryCountsAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating,
        CancellationToken cancellationToken = default)
    {
        // When counting categories, we usually ignore the current category filter but keep others.
        var query = BuildSearchQuery(name, null, null, brandIds, minBasePrice, maxBasePrice, minRating);
        
        return await query
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);
    }

    public async Task<IDictionary<Guid, int>> GetBrandCountsAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating,
        CancellationToken cancellationToken = default)
    {
        // When counting brands, we usually ignore the current brand filter but keep others.
        var query = BuildSearchQuery(name, categoryIds, categorySlugs, null, minBasePrice, maxBasePrice, minRating);
        
        return await query
            .Where(p => p.BrandId != null)
            .GroupBy(p => p.BrandId!.Value)
            .Select(g => new { BrandId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BrandId, x => x.Count, cancellationToken);
    }

    public async Task AddProductAsync(Product product)
    {
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
    }
    public async Task UpdateProductAsync(Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }
    public async Task DeleteProductAsync(Guid productId)
    {
        var product = await context.Products.FindAsync(productId);
        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }
}
