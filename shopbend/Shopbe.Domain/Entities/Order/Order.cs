namespace Shopbe.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid ShippingAddressId { get; set; }
    public Guid? CouponId { get; set; }
    
    // Navigation Properties
    public User? User { get; set; }
    public UserAddress? ShippingAddress { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<OrderStatusHistory> OrderStatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
