using Shopbe.Application.Cart.Dtos;
using Shopbe.Domain.Entities.ShoppingCart;
using System.Linq;

namespace Shopbe.Application.Cart;

public static class CartMapping
{
    public static CartResponseDto ToDto(this ShoppingCart cart)
    {
        var items = cart.CartItems
            .OrderBy(i => i.AddedAt)
            .Select(i => {
                var product = i.ProductVariant?.Product;
                var imageUrl = product?.Images
                    .OrderByDescending(img => img.IsPrimary)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault();

                return new CartItemResponseDto(
                    i.ProductVariantId,
                    product?.Name,
                    imageUrl,
                    i.Quantity,
                    i.UnitPrice,
                    i.UnitPrice * i.Quantity);
            })
            .ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var totalQty = items.Sum(i => i.Quantity);
        var totalItems = items.Count;
        var displayQty = totalQty > 99 ? "99+" : totalQty.ToString();

        return new CartResponseDto(cart.Id, cart.UserId, items, subtotal, totalQty, totalItems, displayQty, "VND");
    }
}

