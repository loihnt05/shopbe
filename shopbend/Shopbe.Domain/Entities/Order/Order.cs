using Shopbe.Domain.Entities.Shipping;
using Shopbe.Domain.Entities.User;

namespace Shopbe.Domain.Entities.Order;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid ShippingAddressId { get; set; }
    public Guid? CouponId { get; set; }
    
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
