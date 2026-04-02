using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IProductImageRepository
{
    Task<IEnumerable<ProductImage>> GetAllProductImagesAsync();
    Task<ProductImage?> GetProductImageByIdAsync(Guid productImageId);
    Task AddProductImageAsync(ProductImage productImage);
    Task UpdateProductImageAsync(ProductImage productImage);
    Task DeleteProductImageAsync(Guid productImageId);
}