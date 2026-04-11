namespace Shopbe.Application.Order.Dtos;

public class CreateOrderRequestDto
{
    /// <summary>
    /// If provided, the order will use this saved user address (must belong to the user).
    /// If null, the handler will try to use the user's default address, unless an override address is provided.
    /// </summary>
    public Guid? UserAddressId { get; set; }

    /// <summary>
    /// When true (default), and UserAddressId is null, the handler will try to use the user's default saved address.
    /// If false, the handler requires the shipping address fields (ShippingReceiverName..ShippingWard).
    /// </summary>
    public bool UseDefaultAddressIfAvailable { get; set; } = true;

    public string ShippingReceiverName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingDistrict { get; set; } = string.Empty;
    public string ShippingWard { get; set; } = string.Empty;
    public string? Note { get; set; }

    /// <summary>
    /// Optional coupon code to apply to this order.
    /// </summary>
    public string? CouponCode { get; set; }
}


