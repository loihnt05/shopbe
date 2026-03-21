namespace Shopbe.Application.ProductsImages.Dtos;
public record ProductImageResponseDto(
    Guid Id,
    string ImageUrl,
    bool IsPrimary
);