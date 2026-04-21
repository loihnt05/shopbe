namespace Shopbe.Application.Review.Dtos;

public sealed record ReviewableProductDto(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    string? ProductImageUrl,
    DateTime PurchasedAt,
    bool IsReviewed,
    Guid? ReviewId);

