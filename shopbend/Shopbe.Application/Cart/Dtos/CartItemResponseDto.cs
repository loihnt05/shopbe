namespace Shopbe.Application.Cart.Dtos;

public sealed record CartItemResponseDto(
    Guid ProductVariantId,
    string? ProductName,
    string? ImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
