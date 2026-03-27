namespace Shopbe.Domain.Entities.Order;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }

    // Navigation properties
    public Order? Order { get; set; }
}

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public bool IsPercentage { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public int MaxUsageCount { get; set; }

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderId { get; set; }

    // Navigation properties
    public Coupon? Coupon { get; set; }
    public User.User? User { get; set; }
    public Order? Order { get; set; }
}

