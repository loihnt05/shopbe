namespace Shopbe.Application.Product.Products.Dtos;

public record ProductQueryDto(
    string? Name,
    IEnumerable<Guid>? CategoryIds,
    IEnumerable<string>? CategorySlugs,
    IEnumerable<Guid>? BrandIds,
    decimal? minBasePrice,
    decimal? maxBasePrice,
    int? MinRating,
    string? SortBy,
    int PageNumber = 1,
    int PageSize = 20
);
