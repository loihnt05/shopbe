namespace Shopbe.Application.Product.ProductImages.Dtos;

public record ProductImageQueryDto(
    Guid? ProductId,
    int PageNumber = 1,
    int PageSize = 20
);