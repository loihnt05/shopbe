using Shopbe.Application.Cart.Dtos;
using Shopbe.Domain.Entities.ShoppingCart;

namespace Shopbe.Application.Cart;

public static class CartMapping
{
    public static CartResponseDto ToDto(this ShoppingCart cart)
    {
        var items = cart.CartItems
            .OrderBy(i => i.AddedAt)
            .Select(i => new CartItemResponseDto(
                i.ProductVariantId,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity))
            .ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var totalQty = items.Sum(i => i.Quantity);

        return new CartResponseDto(cart.Id, cart.UserId, items, subtotal, totalQty);
    }
}

