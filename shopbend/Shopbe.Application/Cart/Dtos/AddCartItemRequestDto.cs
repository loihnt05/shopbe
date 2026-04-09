namespace Shopbe.Application.Cart.Dtos;

public sealed record AddCartItemRequestDto(
    Guid ProductVariantId,
    int Quantity);

