namespace Shopbe.Application.Products.Dtos;

public record ProductImageResponseDto(
    Guid Id,
    string ImageUrl,
    bool IsPrimary
);

public record ProductVariantResponseDto(
    Guid Id,
    string SKU,
    decimal Price,
    int StockQuantity,
    string ImageUrl
);

public record ProductResponseDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string ImageUrl,
    int StockQuantity,
    Guid CategoryId,
    IEnumerable<ProductImageResponseDto> Images,
    IEnumerable<ProductVariantResponseDto> Variants
);
