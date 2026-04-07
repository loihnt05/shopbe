using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IProductVariantRepository
{
    Task<IEnumerable<ProductVariant>> GetAllProductVariantsAsync();
    Task<ProductVariant?> GetProductVariantByIdAsync(Guid productVariantId);
    Task<ProductVariant?> GetProductVariantByIdWithAttributesAsync(Guid productVariantId);
    Task<IEnumerable<ProductVariant>> GetProductVariantsByProductIdAsync(Guid productId);
    Task<bool> ProductVariantSkuExistsAsync(Guid productId, string sku, Guid? excludingVariantId = null);
    Task AddProductVariantAsync(ProductVariant productVariant);
    Task UpdateProductVariantAsync(ProductVariant productVariant);
    Task DeleteProductVariantAsync(Guid productVariantId);
}