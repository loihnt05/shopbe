using Shopbe.Domain.Enums;

namespace Shopbe.Domain.Entities.Seller;

public class SellerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? ShopDescription { get; set; }
    public string? ShopLogoUrl { get; set; }
    public string? ShopBannerUrl { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public SellerStatus Status { get; set; } = SellerStatus.Pending;
    public decimal CommissionRate { get; set; } = 0.05m;
    public decimal? Rating { get; set; }
    public int TotalSales { get; set; } = 0;
    public decimal TotalRevenue { get; set; } = 0;

    // Navigation Properties
    public User.User? User { get; set; }
}
