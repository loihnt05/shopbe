namespace Shopbe.Application.Common.Interfaces.IProduct;

public interface IProductRepository
{
    Task<IEnumerable<Shopbe.Domain.Entities.Product.Product>> GetAllProductsAsync();

    Task<IEnumerable<Shopbe.Domain.Entities.Product.Product>> GetProductsPageAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating,
        string? sortBy,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating,
        CancellationToken cancellationToken = default);

    Task<IDictionary<Guid, int>> GetCategoryCountsAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating,
        CancellationToken cancellationToken = default);

    Task<IDictionary<Guid, int>> GetBrandCountsAsync(
        string? name,
        IEnumerable<Guid>? categoryIds,
        IEnumerable<string>? categorySlugs,
        IEnumerable<Guid>? brandIds,
        decimal? minBasePrice,
        decimal? maxBasePrice,
        int? minRating,
        CancellationToken cancellationToken = default);

    Task<Shopbe.Domain.Entities.Product.Product?> GetProductByIdAsync(Guid productId);
    Task AddProductAsync(Shopbe.Domain.Entities.Product.Product product);
    Task UpdateProductAsync(Shopbe.Domain.Entities.Product.Product product);
    Task DeleteProductAsync(Guid productId);
}
