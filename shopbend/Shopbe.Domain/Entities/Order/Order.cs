using Shopbe.Domain.Entities.Shipping;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;

namespace Shopbe.Domain.Entities.Order;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public string ShippingReceiverName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingDistrict { get; set; } = string.Empty;
    public string ShippingWard { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }
    
    // Navigation Properties
    public User.User? User { get; set; }
    public UserAddress? ShippingAddress { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment.Payment> Payments { get; set; } = new List<Payment.Payment>();
    public ICollection<OrderStatusHistory> OrderStatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
