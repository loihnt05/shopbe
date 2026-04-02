using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ProductRepositories;

public class ProductVariantRepository(ShopDbContext context) : IProductVariantRepository
{
    public async Task<ProductVariant?> GetProductVariantByIdAsync(Guid productVariantId)
    {
        return await context.ProductVariants.FindAsync(productVariantId);
    }
    public async Task<IEnumerable<ProductVariant>> GetProductVariantsByProductIdAsync(Guid productId)
    {
        return await context.ProductVariants
            .Where(pv => pv.ProductId == productId)
            .ToListAsync();
    }
    public async Task<IEnumerable<ProductVariant>> GetAllProductVariantsAsync()
    {
        return await context.ProductVariants.ToListAsync();
    }
    public async Task AddProductVariantAsync(ProductVariant productVariant)
    {
        await context.ProductVariants.AddAsync(productVariant);
    }
    public async Task UpdateProductVariantAsync(ProductVariant productVariant)
    {
        context.ProductVariants.Update(productVariant);
    }
    public async Task DeleteProductVariantAsync(Guid productVariantId)
    {
        var productVariant = await context.ProductVariants.FindAsync(productVariantId);
        if (productVariant != null)
        {
            context.ProductVariants.Remove(productVariant);
        }
    }
}