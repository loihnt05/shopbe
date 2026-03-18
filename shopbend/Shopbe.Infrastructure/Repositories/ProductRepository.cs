using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Interfaces;
using Shopbe.Domain.Entities;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ShopDbContext _context;
    public ProductRepository(ShopDbContext context)
    {
        _context = context;
    }
    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .ToListAsync();
    }
    public async Task AddProductAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }
    public async Task UpdateProductAsync(Product product)
    {
        _context.Products.Update(product);
    }
    public async Task DeleteProductAsync(Guid productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            _context.Products.Remove(product);
        }
    }
}