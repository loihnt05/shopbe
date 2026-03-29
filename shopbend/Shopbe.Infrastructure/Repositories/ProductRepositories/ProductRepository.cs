using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Interfaces;
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
    public async Task AddProductAsync(Product product)
    {
        await context.Products.AddAsync(product);
    }
    public async Task UpdateProductAsync(Product product)
    {
        context.Products.Update(product);
    }
    public async Task DeleteProductAsync(Guid productId)
    {
        var product = await context.Products.FindAsync(productId);
        if (product != null)
        {
            context.Products.Remove(product);
        }
    }
}