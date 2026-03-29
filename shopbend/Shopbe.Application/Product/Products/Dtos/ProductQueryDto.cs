namespace Shopbe.Application.Products.Dtos;

public record ProductQueryDto(
    string? Name,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int PageNumber = 1,
    int PageSize = 20
);
