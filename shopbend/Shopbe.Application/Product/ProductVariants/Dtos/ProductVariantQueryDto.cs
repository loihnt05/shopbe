namespace Shopbe.Application.ProductVariants.Dtos;

public record ProductVariantQueryDto(
    string? SKU,
    decimal? MinPrice,
    decimal? MaxPrice,
    int PageNumber = 1,
    int PageSize = 20
);