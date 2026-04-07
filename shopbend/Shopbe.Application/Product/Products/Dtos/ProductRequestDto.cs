using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.Products.Dtos;

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
