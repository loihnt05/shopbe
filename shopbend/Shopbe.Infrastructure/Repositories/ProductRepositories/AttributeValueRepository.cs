using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Domain.Entities.Product;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ProductRepositories;

public class AttributeValueRepository(ShopDbContext context) : IAttributeValueRepository
{
    public async Task<IEnumerable<AttributeValue>> GetAllAsync()
    {
        return await context.AttributeValues
            .AsNoTracking()
            .OrderBy(v => v.Value)
            .ToListAsync();
    }

    public async Task<IEnumerable<AttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId)
    {
        return await context.AttributeValues
            .AsNoTracking()
            .Where(v => v.AttributeId == attributeId)
            .OrderBy(v => v.Value)
            .ToListAsync();
    }

    public async Task<AttributeValue?> GetValueByIdAsync(Guid attributeValueId)
    {
        return await context.AttributeValues
            .FirstOrDefaultAsync(v => v.Id == attributeValueId);
    }

    public async Task<AttributeValue?> GetValueAsync(Guid attributeId, string value)
    {
        var normalized = Normalize(value);
        return await context.AttributeValues
            .FirstOrDefaultAsync(v => v.AttributeId == attributeId && v.Value.ToLower() == normalized);
    }

    public async Task<IEnumerable<AttributeValue>> GetValuesByIdsAsync(IEnumerable<Guid> attributeValueIds)
    {
        var ids = attributeValueIds.ToList();
        if (ids.Count == 0) return [];

        return await context.AttributeValues
            .AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .ToListAsync();
    }

    public async Task AddValueAsync(AttributeValue attributeValue)
    {
        await context.AttributeValues.AddAsync(attributeValue);
        await context.SaveChangesAsync();
    }

    public async Task UpdateValueAsync(AttributeValue attributeValue)
    {
        context.AttributeValues.Update(attributeValue);
        await context.SaveChangesAsync();
    }

    public async Task DeleteValueAsync(Guid attributeValueId)
    {
        var attributeValue = await context.AttributeValues.FindAsync(attributeValueId);
        if (attributeValue != null)
        {
            context.AttributeValues.Remove(attributeValue);
            await context.SaveChangesAsync();
        }
    }

    private static string Normalize(string input) => input.Trim().ToLower();
}


