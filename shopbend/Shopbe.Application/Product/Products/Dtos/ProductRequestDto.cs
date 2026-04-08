using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.Products.Dtos;

public record ProductRequestDto(
    string Name,
    string Description,
    decimal BasePrice,
    Guid CategoryId,
    Guid? BrandId,
    bool IsActive,
    IEnumerable<ProductImageRequestDto>? Images,
    IEnumerable<ProductVariantRequestDto>? Variants
);
