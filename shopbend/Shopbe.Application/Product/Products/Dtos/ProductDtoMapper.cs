using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;

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

        // Domain has no product-level stock; expose a computed sum for convenience.
        var totalStockQuantity = product.Variants.Sum(v => v.StockQuantity);

        var variants = product.Variants
            .Select(v => new ProductVariantResponseDto(
                v.Id,
                v.ProductId,
                v.Sku,
                v.Price,
                v.StockQuantity,
                v.IsActive,
                v.ProductVariantAttributes
                    .Select(a => a.AttributeValueId)
                    .Distinct()
                    .ToList()
            ))
            .ToList();

        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Slug,
            product.Description ?? string.Empty,
            product.BasePrice,
            primaryImageUrl,
            totalStockQuantity,
            product.CategoryId,
            product.BrandId,
            product.IsActive,
            images,
            variants
        );
    }
}
