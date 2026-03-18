namespace Shopbe.Application.Products.Dtos;

public record ProductImageRequestDto(
    string ImageUrl,
    bool IsPrimary
);

public record ProductVariantRequestDto(
    string SKU,
    decimal Price,
    int StockQuantity,
    string ImageUrl
);

public record ProductRequestDto(
    string Name,
    string Description,
    decimal Price,
    string ImageUrl,
    int StockQuantity,
    Guid CategoryId,
    IEnumerable<ProductImageRequestDto>? Images,
    IEnumerable<ProductVariantRequestDto>? Variants
);
