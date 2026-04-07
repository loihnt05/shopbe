namespace Shopbe.Application.Product.ProductImages.Dtos;
public record ProductImageRequestDto(
    string ImageUrl,
    bool IsPrimary
);
