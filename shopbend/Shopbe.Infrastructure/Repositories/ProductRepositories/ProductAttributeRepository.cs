using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ProductRepositories;

public class ProductAttributeRepository(ShopDbContext context) : IProductAttributeRepository
{
    public async Task<IEnumerable<ProductAttribute>> GetAllAttributesAsync()
    {
        return await context.ProductAttributes
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<ProductAttribute?> GetAttributeByIdAsync(Guid attributeId)
    {
        return await context.ProductAttributes
            .Include(a => a.AttributeValues)
            .FirstOrDefaultAsync(a => a.Id == attributeId);
    }

    public async Task<ProductAttribute?> GetAttributeByNameAsync(string name)
    {
        var normalized = Normalize(name);
        return await context.ProductAttributes
            .FirstOrDefaultAsync(a => a.Name.ToLower() == normalized);
    }

    public async Task AddAttributeAsync(ProductAttribute attribute)
    {
        await context.ProductAttributes.AddAsync(attribute);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAttributeAsync(ProductAttribute attribute)
    {
        context.ProductAttributes.Update(attribute);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAttributeAsync(Guid attributeId)
    {
        var attribute = await context.ProductAttributes.FindAsync(attributeId);
        if (attribute != null)
        {
            context.ProductAttributes.Remove(attribute);
            await context.SaveChangesAsync();
        }
    }

    private static string Normalize(string input) => input.Trim().ToLower();
}


