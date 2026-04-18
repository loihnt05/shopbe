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
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsPageAsync(
        string? name,
        Guid? categoryId,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            // Use ILIKE for Postgres when possible (EF.Functions) for efficient case-insensitive search.
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{name}%"));
        }

        if (categoryId is not null)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (minBasePrice is not null)
        {
            query = query.Where(p => p.BasePrice >= minBasePrice);
        }

        if (maxBasePrice is not null)
        {
            query = query.Where(p => p.BasePrice <= maxBasePrice);
        }

        // Stable order for paging.
        query = query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id);

        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
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