using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IProductAttributeRepository
{
    Task<IEnumerable<ProductAttribute>> GetAllAttributesAsync();
    Task<ProductAttribute?> GetAttributeByIdAsync(Guid attributeId);
    Task<ProductAttribute?> GetAttributeByNameAsync(string name);
    Task AddAttributeAsync(ProductAttribute attribute);
    Task UpdateAttributeAsync(ProductAttribute attribute);
    Task DeleteAttributeAsync(Guid attributeId);
}

