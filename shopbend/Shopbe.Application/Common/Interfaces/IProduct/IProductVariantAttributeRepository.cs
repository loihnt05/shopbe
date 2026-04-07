using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IProductVariantAttributeRepository
{
    Task<IEnumerable<ProductVariantAttribute>> GetByVariantIdAsync(Guid variantId);
    Task AddRangeAsync(IEnumerable<ProductVariantAttribute> items);
    Task DeleteByVariantIdAsync(Guid variantId);
}

