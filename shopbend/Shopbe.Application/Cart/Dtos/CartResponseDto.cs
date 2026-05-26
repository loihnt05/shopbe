namespace Shopbe.Application.Cart.Dtos;

public sealed record CartResponseDto(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemResponseDto> Items,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    string? CouponCode,
    int TotalQuantity,
    int TotalItems,
    string DisplayQuantity,
    string Currency = "VND");
