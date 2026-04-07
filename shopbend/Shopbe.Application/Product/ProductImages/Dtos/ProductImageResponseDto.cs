namespace Shopbe.Application.Product.ProductImages.Dtos;
public record ProductImageResponseDto(
    Guid Id,
    string ImageUrl,
    bool IsPrimary
);