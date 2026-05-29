using Shopbe.Application.Wishlist.Dtos;
using Shopbe.Domain.Entities.Wishlist;
using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Wishlist;

public static class WishlistMapping
{
    public static WishlistItemResponseDto ToDto(this WishlistItem item)
    {
        var product = item.Product;
        if (product == null)
        {
            return new WishlistItemResponseDto(
                item.Id,
                item.UserId,
                item.ProductId,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                null,
                "VND",
                string.Empty,
                0,
                0,
                0,
                0,
                Guid.Empty,
                null,
                null,
                null,
                false,
                Enumerable.Empty<ProductImageResponseDto>(),
                Enumerable.Empty<ProductVariantResponseDto>()
            );
        }

        var primaryImageUrl = product.Images?
            .OrderByDescending(i => i.IsPrimary)
            .Select(i => i.ImageUrl)
            .FirstOrDefault() ?? string.Empty;

        var images = product.Images?
            .OrderByDescending(i => i.IsPrimary)
            .Select(i => new ProductImageResponseDto(i.Id, i.ImageUrl, i.IsPrimary))
            .ToList() ?? new List<ProductImageResponseDto>();

        var totalStockQuantity = product.Variants?.Sum(v => v.StockQuantity) ?? 0;
        
        var variants = product.Variants?
            .Select(v => new ProductVariantResponseDto(
                v.Id,
                v.ProductId,
                v.Sku,
                v.Price,
                "VND",
                v.StockQuantity,
                v.IsActive,
                v.ProductVariantAttributes?
                    .Select(a => {
                        var attrName = a.AttributeValue?.Attribute?.Name ?? "Option";
                        var attrValue = a.AttributeValue?.Value ?? string.Empty;
                        return new ProductVariantAttributeResponseDto(attrName, attrValue);
                    })
                    .Where(a => !string.IsNullOrEmpty(a.Value))
                    .ToList() ?? new List<ProductVariantAttributeResponseDto>()
            ))
            .ToList() ?? new List<ProductVariantResponseDto>();

        var averageRating = product.Reviews != null && product.Reviews.Any() 
            ? product.Reviews.Average(r => r.Rating) 
            : 0;
        
        var ratingCount = product.Reviews?.Count ?? 0;

        return new WishlistItemResponseDto(
            item.Id,
            item.UserId,
            item.ProductId,
            product.Name,
            product.Slug,
            product.Description ?? string.Empty,
            product.BasePrice,
            product.DiscountPrice,
            "VND",
            primaryImageUrl,
            totalStockQuantity,
            product.SoldCount,
            averageRating,
            ratingCount,
            product.CategoryId,
            product.Category?.Name,
            product.BrandId,
            product.Brand?.Name,
            product.IsActive,
            images,
            variants
        );
    }
}
