using Shopbe.Application.Cart.Dtos;
using Shopbe.Domain.Entities.ShoppingCart;
using Shopbe.Domain.Enums;
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

        decimal discountAmount = 0;
        if (cart.Coupon != null && cart.Coupon.IsActive && cart.Coupon.Count > 0 && cart.Coupon.ExpiredAt > DateTime.UtcNow && subtotal >= cart.Coupon.MinOrderAmount)
        {
            if (cart.Coupon.DiscountType == Shopbe.Domain.Enums.DiscountType.Percentage)
            {
                discountAmount = subtotal * (cart.Coupon.Value / 100);
                if (cart.Coupon.MaxDiscountAmount.HasValue && discountAmount > cart.Coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = cart.Coupon.MaxDiscountAmount.Value;
                }
            }
            else if (cart.Coupon.DiscountType == Shopbe.Domain.Enums.DiscountType.FixedAmount)
            {
                discountAmount = cart.Coupon.Value;
            }
            
            if (discountAmount > subtotal)
            {
                discountAmount = subtotal;
            }
        }

        var total = subtotal - discountAmount;

        return new CartResponseDto(
            cart.Id, 
            cart.UserId, 
            items, 
            subtotal, 
            discountAmount, 
            total, 
            cart.Coupon?.Code, 
            totalQty, 
            totalItems, 
            displayQty, 
            "VND");
    }
}

