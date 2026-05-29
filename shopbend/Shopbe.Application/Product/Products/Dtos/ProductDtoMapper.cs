using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Shopbe.Application.Product.Products.Dtos;

public static class ProductDtoMapper
{
    public static ProductResponseDto ToResponse(Domain.Entities.Product.Product product, string? reason = null)
    {
        var primaryImageUrl = product.Images
            .OrderByDescending(i => i.IsPrimary)
            .Select(i => i.ImageUrl)
            .FirstOrDefault() ?? string.Empty;

        var images = product.Images
            .OrderByDescending(i => i.IsPrimary)
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
                "VND",
                v.StockQuantity,
                v.IsActive,
                v.ProductVariantAttributes
                    .Select(a => {
                        var attrName = a.AttributeValue?.Attribute?.Name;
                        var attrValue = a.AttributeValue?.Value;
                        
                        // Fallback logic if navigation properties are missing but IDs are present
                        if (string.IsNullOrEmpty(attrName)) {
                            attrName = "Option";
                        }
                        
                        return new ProductVariantAttributeResponseDto(attrName, attrValue ?? string.Empty);
                    })
                    .Where(a => !string.IsNullOrEmpty(a.Value))
                    .ToList()
            ))
            .ToList();

        var averageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0;
        var ratingCount = product.Reviews.Count;

        return new ProductResponseDto(
            product.Id,
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
            variants,
            reason
        );
    }

    public static ProductResponseDto MapFromDummy(DummyProductDto dp)
    {
        var productId = Guid.NewGuid(); // Or use a deterministic GUID from dp.Id if preferred
        var categoryId = GuidFromName(dp.Category);
        var brandId = !string.IsNullOrEmpty(dp.Brand) ? GuidFromName(dp.Brand) : (Guid?)null;
        
        var discountPrice = dp.DiscountPercentage > 0 
            ? Math.Round(dp.Price * (1 - dp.DiscountPercentage / 100), 2) 
            : (decimal?)null;

        // Realistic sold count: higher rating generally means more sales.
        // Also add some randomness.
        var baseSold = (int)(dp.Rating * 100);
        var soldCount = baseSold + RandomNumberGenerator.GetInt32(0, 500);

        var images = new List<ProductImageResponseDto>();
        if (dp.Images != null)
        {
            images.AddRange(dp.Images.Select((url, index) => 
                new ProductImageResponseDto(Guid.NewGuid(), url, index == 0)));
        }
        else if (!string.IsNullOrEmpty(dp.Thumbnail))
        {
            images.Add(new ProductImageResponseDto(Guid.NewGuid(), dp.Thumbnail, true));
        }

        var primaryImageUrl = images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? dp.Thumbnail ?? string.Empty;

        var variant = new ProductVariantResponseDto(
            Guid.NewGuid(),
            productId,
            dp.Sku ?? $"SKU-{dp.Id}",
            dp.Price,
            "USD",
            dp.Stock,
            true,
            new List<ProductVariantAttributeResponseDto>() // Fake attributes
        );

        return new ProductResponseDto(
            productId,
            dp.Title,
            Slugify(dp.Title),
            dp.Description,
            dp.Price,
            discountPrice,
            "USD",
            primaryImageUrl,
            dp.Stock,
            soldCount,
            (double)dp.Rating,
            dp.Rating > 0 ? RandomNumberGenerator.GetInt32(5, 100) : 0,
            categoryId,
            dp.Category,
            brandId,
            dp.Brand,
            true,
            images,
            new[] { variant }
        );
    }

    private static string Slugify(string value)
    {
        var s = value.Trim().ToLowerInvariant();
        var chars = s.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-", StringComparison.Ordinal);
        return slug.Trim('-');
    }

    private static Guid GuidFromName(string name)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(name));
        return new Guid(hash);
    }
}
