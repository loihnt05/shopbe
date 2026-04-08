using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.Products.Dtos;

public record ProductResponseDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    decimal BasePrice,
    string PrimaryImageUrl,
    int TotalStockQuantity,
    Guid CategoryId,
    Guid? BrandId,
    bool IsActive,
    IEnumerable<ProductImageResponseDto> Images,
    IEnumerable<ProductVariantResponseDto> Variants
);
