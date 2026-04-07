using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ProductRepositories;

public class ProductVariantRepository(ShopDbContext context) : IProductVariantRepository
{
    public async Task<ProductVariant?> GetProductVariantByIdAsync(Guid productVariantId)
    {
        return await context.ProductVariants
            .AsNoTracking()
            .FirstOrDefaultAsync(pv => pv.Id == productVariantId && pv.DeletedAt == null);
    }

    public async Task<ProductVariant?> GetProductVariantByIdWithAttributesAsync(Guid productVariantId)
    {
        return await context.ProductVariants
            .Include(pv => pv.ProductVariantAttributes)
            .ThenInclude(pva => pva.AttributeValue)
            .FirstOrDefaultAsync(pv => pv.Id == productVariantId && pv.DeletedAt == null);
    }

    public async Task<IEnumerable<ProductVariant>> GetProductVariantsByProductIdAsync(Guid productId)
    {
        return await context.ProductVariants
            .AsNoTracking()
            .Where(pv => pv.ProductId == productId && pv.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductVariant>> GetAllProductVariantsAsync()
    {
        return await context.ProductVariants
            .AsNoTracking()
            .Where(pv => pv.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<bool> ProductVariantSkuExistsAsync(Guid productId, string sku, Guid? excludingVariantId = null)
    {
        var normalized = sku.Trim();
        return await context.ProductVariants
            .AsNoTracking()
            .Where(pv => pv.ProductId == productId && pv.DeletedAt == null)
            .Where(pv => excludingVariantId == null || pv.Id != excludingVariantId)
            .AnyAsync(pv => pv.Sku == normalized);
    }

    public async Task AddProductVariantAsync(ProductVariant productVariant)
    {
        await context.ProductVariants.AddAsync(productVariant);
        await context.SaveChangesAsync();
    }
    public async Task UpdateProductVariantAsync(ProductVariant productVariant)
    {
        context.ProductVariants.Update(productVariant);
        await context.SaveChangesAsync();
    }
    public async Task DeleteProductVariantAsync(Guid productVariantId)
    {
        var productVariant = await context.ProductVariants.FindAsync(productVariantId);
        if (productVariant != null)
        {
            productVariant.DeletedAt = DateTime.UtcNow;
            context.ProductVariants.Update(productVariant);
            await context.SaveChangesAsync();
        }
    }
}