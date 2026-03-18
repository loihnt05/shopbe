using Shopbe.Domain.Entities;

namespace Shopbe.Application.Products.Dtos;

public static class ProductDtoMapper
{
    public static ProductResponseDto ToResponse(Product product)
    {
        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.StockQuantity,
            product.CategoryId,
            product.Images.Select(i => new ProductImageResponseDto(
                i.Id,
                i.ImageUrl,
                i.IsPrimary
            )),
            product.Variants.Select(v => new ProductVariantResponseDto(
                v.Id,
                v.SKU,
                v.Price,
                v.StockQuantity,
                v.ImageUrl
            ))
        );
    }
}
