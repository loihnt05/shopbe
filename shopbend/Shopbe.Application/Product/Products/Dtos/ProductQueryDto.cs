namespace Shopbe.Application.Product.Products.Dtos;

public record ProductQueryDto(
    string? Name,
    IEnumerable<Guid>? CategoryIds,
    decimal? MinBasePrice,
    decimal? MaxBasePrice,
    int PageNumber = 1,
    int PageSize = 20
);
