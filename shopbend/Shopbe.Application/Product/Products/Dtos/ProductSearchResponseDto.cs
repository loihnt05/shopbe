namespace Shopbe.Application.Product.Products.Dtos;

public record CategoryFacetDto(Guid Id, string Name, string Slug, int Count);

public record ProductSearchResponseDto(
    IEnumerable<ProductResponseDto> Products,
    IEnumerable<CategoryFacetDto> CategoryFacets,
    int TotalCount
);
