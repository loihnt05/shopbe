namespace Shopbe.Application.Cart.Dtos;

public sealed record CartResponseDto(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemResponseDto> Items,
    decimal Subtotal,
    int TotalQuantity,
    int TotalItems,
    string DisplayQuantity,
    string Currency = "VND");
