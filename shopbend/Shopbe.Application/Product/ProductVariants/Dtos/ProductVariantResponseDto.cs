namespace Shopbe.Application.Product.ProductVariants.Dtos;

public record ProductVariantResponseDto(
    Guid Id,
    Guid ProductId,
    string SKU,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    IReadOnlyCollection<Guid> AttributeValueIds
);