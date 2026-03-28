using Shopbe.Domain.Entities.Admin;
using Shopbe.Domain.Entities.Chatbot;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Recommendation;
using Shopbe.Domain.Entities.Wishlist;

namespace Shopbe.Domain.Entities.User;

public class User : BaseEntity
{
    // Chỉ cần ExternalId để map với Keycloak subject (sub claim)
    public string KeycloakId { get; set; } = string.Empty;  // = sub claim
    
    // Email cache lại để query nhanh, không cần join Keycloak
    public string Email { get; set; } = string.Empty;
    
    // Role quản lý ở app level (hoặc sync từ Keycloak tùy chiến lược)
    public UserRole Role { get; set; } = UserRole.Customer;
    public string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public UserStatus Status { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public ShoppingCart.ShoppingCart? ShoppingCart { get; set; }
    public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    public ICollection<Order.Order> Orders { get; set; } = new List<Order.Order>();
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public ICollection<Review.Review> Reviews { get; set; } = new List<Review.Review>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<UserBehavior> UserBehaviors { get; set; } = new List<UserBehavior>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Notification.Notification> Notifications { get; set; } = new List<Notification.Notification>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}