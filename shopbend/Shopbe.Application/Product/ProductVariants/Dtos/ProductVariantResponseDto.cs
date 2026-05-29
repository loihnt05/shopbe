namespace Shopbe.Application.Product.ProductVariants.Dtos;

public record ProductVariantAttributeResponseDto(
    string Name,
    string Value
);

public record ProductVariantResponseDto(
    Guid Id,
    Guid ProductId,
    string SKU,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    IReadOnlyCollection<ProductVariantAttributeResponseDto> Attributes
);