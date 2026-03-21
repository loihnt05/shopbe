namespace Shopbe.Application.ProductVariants.Dtos;
public record ProductVariantRequestDto(
    string SKU,
    decimal Price,
    int StockQuantity,
    string ImageUrl
);