using Shopbe.Application.ProductsImages.Dtos;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.Product.Products.Dtos;

public static class ProductDtoMapper
{
    public static ProductResponseDto ToResponse(Domain.Entities.Product.Product product)
    {
        var primaryImageUrl = product.Images
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefault() ?? string.Empty;

        var images = product.Images
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .Select(i => new ProductImageResponseDto(i.Id, i.ImageUrl, i.IsPrimary))
            .ToList();

        // If the product uses variants, treat total stock as the sum of variant stock.
        // Otherwise, fall back to 0 (domain product currently has no StockQuantity).
        var stockQuantity = product.Variants.Sum(v => v.StockQuantity);

        var variants = product.Variants
            .Select(v => new ProductVariantResponseDto(
                v.Id,
                v.Sku,
                v.Price,
                v.StockQuantity,
                ImageUrl: primaryImageUrl
            ))
            .ToList();

        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description ?? string.Empty,
            product.BasePrice,
            primaryImageUrl,
            stockQuantity,
            product.CategoryId,
            images,
            variants
        );
    }
}
