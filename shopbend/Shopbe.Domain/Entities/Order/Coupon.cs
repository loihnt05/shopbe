namespace Shopbe.Domain.Entities.Order;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime ExpiredAt { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}

