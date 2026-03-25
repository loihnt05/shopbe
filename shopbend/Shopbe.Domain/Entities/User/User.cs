namespace Shopbe.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    
    // Navigation Properties
    public UserProfile? Profile { get; set; }
    public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ShoppingCart? ShoppingCart { get; set; }
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<UserBehavior> UserBehaviors { get; set; } = new List<UserBehavior>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}