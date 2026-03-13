namespace Shopbe.Application.Products.Dtos;

public record ProductRequestDto(
    string Name,
    string Description,
    decimal Price,
    string ImageUrl,
    int StockQuantity,
    Guid CategoryId
);
