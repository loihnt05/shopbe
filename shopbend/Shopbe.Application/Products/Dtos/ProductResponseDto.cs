using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.Products.Dtos;

public record ProductImageResponseDto(
    Guid Id,
    string ImageUrl,
    bool IsPrimary
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
