namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IProductRepository
{
    Task<IEnumerable<Shopbe.Domain.Entities.Product.Product>> GetAllProductsAsync();
    Task<Shopbe.Domain.Entities.Product.Product?> GetProductByIdAsync(Guid productId);
    Task AddProductAsync(Shopbe.Domain.Entities.Product.Product product);
    Task UpdateProductAsync(Shopbe.Domain.Entities.Product.Product product);
    Task DeleteProductAsync(Guid productId);
}