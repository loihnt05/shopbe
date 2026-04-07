using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.Products.Dtos;

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
