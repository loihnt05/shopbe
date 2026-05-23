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
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.ProductVariantAttributes)
                    .ThenInclude(pva => pva.AttributeValue)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.ProductVariantAttributes)
                    .ThenInclude(pva => pva.AttributeValue)
            .ToListAsync();
    }

    private IQueryable<Product> BuildSearchQuery(
        string? name,
        IEnumerable<Guid>? categoryIds,
        decimal? minBasePrice,
        decimal? maxBasePrice)
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

        if (minBasePrice is not null)
        {
            query = query.Where(p => p.BasePrice >= minBasePrice);
        }

        if (maxBasePrice is not null)
        {
            query = query.Where(p => p.BasePrice <= maxBasePrice);
        }

        return query;
    }

    public async Task<IEnumerable<Product>> GetProductsPageAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = BuildSearchQuery(name, categoryIds, minBasePrice, maxBasePrice)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.ProductVariantAttributes)
                    .ThenInclude(pva => pva.AttributeValue);

        var isFiltered = !string.IsNullOrWhiteSpace(name) || 
                        (categoryIds != null && categoryIds.Any()) || 
                        minBasePrice != null || 
                        maxBasePrice != null;

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;

        if (!isFiltered)
        {
            query = query.OrderBy(p => Guid.NewGuid());
        }
        else
        {
            query = query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id);
        }

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        CancellationToken cancellationToken = default)
    {
        return await BuildSearchQuery(name, categoryIds, minBasePrice, maxBasePrice)
            .CountAsync(cancellationToken);
    }

    public async Task<IDictionary<Guid, int>> GetCategoryCountsAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        CancellationToken cancellationToken = default)
    {
        // For category counts, we want to know how many products match the current filters (EXCEPT the category filter itself, often, but let's stick to matching ALL filters for now as requested).
        // Actually, usually when you filter by category A, you still want to see counts for category B.
        // But for simplicity and matching the prompt "Display the number of matching products for each category", I'll matching current search + other filters.
        
        var query = BuildSearchQuery(name, null, minBasePrice, maxBasePrice); // Ignore categoryIds for facets to show other categories too? 
        // No, the prompt says "matching products". If I have 10 products in Shoes and I filter by Apparel, it should probably show Shoes(10) still if that's how the sidebar works, or Shoes(0) if it's strictly matching.
        // Let's go with matching ALL filters including name, but excluding the category filter itself so we can see other categories.
        
        return await query
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);
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