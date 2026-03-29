using Shopbe.Application.ProductsImages.Dtos;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.Products.Dtos;

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
