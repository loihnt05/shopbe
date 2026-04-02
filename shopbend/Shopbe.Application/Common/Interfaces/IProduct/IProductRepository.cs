using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(Guid productId);
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Guid productId);
}