namespace Shopbe.Application.Products.Dtos;

public record ProductResponseDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string ImageUrl,
    int StockQuantity,
    Guid CategoryId
);
