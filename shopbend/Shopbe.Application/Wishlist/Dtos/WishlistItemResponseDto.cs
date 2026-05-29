using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Wishlist.Dtos;

public sealed record WishlistItemResponseDto(
    Guid Id,
    Guid UserId,
    Guid ProductId,
    string Name,
    string Slug,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string Currency,
    string PrimaryImageUrl,
    int TotalStockQuantity,
    int SoldCount,
    double AverageRating,
    int RatingCount,
    Guid CategoryId,
    string? CategoryName,
    Guid? BrandId,
    string? BrandName,
    bool IsActive,
    IEnumerable<ProductImageResponseDto> Images,
    IEnumerable<ProductVariantResponseDto> Variants);
