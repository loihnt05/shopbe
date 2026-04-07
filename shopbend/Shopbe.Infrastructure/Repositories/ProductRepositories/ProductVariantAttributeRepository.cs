using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ProductRepositories;

public class ProductVariantAttributeRepository(ShopDbContext context) : IProductVariantAttributeRepository
{
    public async Task<IEnumerable<ProductVariantAttribute>> GetByVariantIdAsync(Guid variantId)
    {
        return await context.ProductVariantAttributes
            .AsNoTracking()
            .Where(x => x.VariantId == variantId)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<ProductVariantAttribute> items)
    {
        await context.ProductVariantAttributes.AddRangeAsync(items);
    }

    public async Task DeleteByVariantIdAsync(Guid variantId)
    {
        var rows = await context.ProductVariantAttributes
            .Where(x => x.VariantId == variantId)
            .ToListAsync();

        if (rows.Count == 0)
        {
            return;
        }

        context.ProductVariantAttributes.RemoveRange(rows);
    }
}

