namespace Shopbe.Application.ProductVariants.Dtos;
public record ProductVariantResponseDto(
    Guid Id,
    string SKU,
    decimal Price,
    int StockQuantity,
    string ImageUrl
);