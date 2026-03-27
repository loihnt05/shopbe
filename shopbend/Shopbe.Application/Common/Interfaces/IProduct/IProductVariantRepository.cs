using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Interfaces.Repositories;

public interface IProductVariantRepository
{
    Task<IEnumerable<ProductVariant>> GetAllProductVariantsAsync();
    Task<ProductVariant?> GetProductVariantByIdAsync(Guid productVariantId);
    Task AddProductVariantAsync(ProductVariant productVariant);
    Task UpdateProductVariantAsync(ProductVariant productVariant);
    Task DeleteProductVariantAsync(Guid productVariantId);
}