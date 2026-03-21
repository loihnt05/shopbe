namespace Shopbe.Application.ProductsImages.Dtos;
public record ProductImageRequestDto(
    string ImageUrl,
    bool IsPrimary
);
