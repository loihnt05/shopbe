using Shopbe.Application.ProductsImages.Dtos;
using Shopbe.Application.ProductVariants.Dtos;
using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Products.Dtos;

public static class ProductDtoMapper
{
    public static ProductResponseDto ToResponse(Product product)
    {
        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description ?? string.Empty,
            product.BasePrice,
            primaryImageUrl,
            product.Variants.Sum(v => v.StockQuantity),
            product.CategoryId,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.StockQuantity,
            )),
            product.Variants.Select(v => new ProductVariantResponseDto(
                v.Id,
                v.SKU,
                v.Sku,
                v.Price,
                v.StockQuantity,
                v.ImageUrl
                primaryImageUrl
            ))
        );
    }
}
