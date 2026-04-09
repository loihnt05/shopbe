namespace Shopbe.Application.Cart.Dtos;

public sealed record CartItemResponseDto(
    Guid ProductVariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

