using Shopbe.Domain.Entities.User;

namespace Shopbe.Domain.Entities.Order;

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public DateTime UsedAt { get; set; }

    // Navigation Properties
    public Coupon? Coupon { get; set; }
    public User.User? User { get; set; }
    public Order? Order { get; set; }
}

