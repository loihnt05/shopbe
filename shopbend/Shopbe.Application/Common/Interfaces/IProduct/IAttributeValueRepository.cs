using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IAttributeValueRepository
{
    Task<IEnumerable<AttributeValue>> GetValuesByAttributeIdAsync(Guid attributeId);
    Task<AttributeValue?> GetValueByIdAsync(Guid attributeValueId);
    Task<AttributeValue?> GetValueAsync(Guid attributeId, string value);
    Task<IEnumerable<AttributeValue>> GetValuesByIdsAsync(IEnumerable<Guid> attributeValueIds);
    Task AddValueAsync(AttributeValue attributeValue);
    Task UpdateValueAsync(AttributeValue attributeValue);
    Task DeleteValueAsync(Guid attributeValueId);
}

