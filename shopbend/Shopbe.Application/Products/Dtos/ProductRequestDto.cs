using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.Products.Dtos;

public record ProductImageRequestDto(
    string ImageUrl,
    bool IsPrimary
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
