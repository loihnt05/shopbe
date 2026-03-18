using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Interfaces.Repositories;
using Shopbe.Domain.Entities;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories;

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly ShopDbContext _context;
    public ProductVariantRepository(ShopDbContext context)
    {
        _context = context;
    }
    public async Task<ProductVariant?> GetProductVariantByIdAsync(Guid productVariantId)
    {
        return await _context.ProductVariants.FindAsync(productVariantId);
    }
    public async Task<IEnumerable<ProductVariant>> GetProductVariantsByProductIdAsync(Guid productId)
    {
        return await _context.ProductVariants
            .Where(pv => pv.ProductId == productId)
            .ToListAsync();
    }
    public async Task<IEnumerable<ProductVariant>> GetAllProductVariantsAsync()
    {
        return await _context.ProductVariants.ToListAsync();
    }
    public async Task AddProductVariantAsync(ProductVariant productVariant)
    {
        await _context.ProductVariants.AddAsync(productVariant);
    }
    public async Task UpdateProductVariantAsync(ProductVariant productVariant)
    {
        _context.ProductVariants.Update(productVariant);
    }
    public async Task DeleteProductVariantAsync(Guid productVariantId)
    {
        var productVariant = await _context.ProductVariants.FindAsync(productVariantId);
        if (productVariant != null)
        {
            _context.ProductVariants.Remove(productVariant);
        }
    }
}