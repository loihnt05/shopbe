namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IProductRepository
{
    Task<IEnumerable<Shopbe.Domain.Entities.Product.Product>> GetAllProductsAsync();

    Task<IEnumerable<Shopbe.Domain.Entities.Product.Product>> GetProductsPageAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        CancellationToken cancellationToken = default);

    Task<IDictionary<Guid, int>> GetCategoryCountsAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        CancellationToken cancellationToken = default);

    Task<Shopbe.Domain.Entities.Product.Product?> GetProductByIdAsync(Guid productId);
    Task AddProductAsync(Shopbe.Domain.Entities.Product.Product product);
    Task UpdateProductAsync(Shopbe.Domain.Entities.Product.Product product);
    Task DeleteProductAsync(Guid productId);
}