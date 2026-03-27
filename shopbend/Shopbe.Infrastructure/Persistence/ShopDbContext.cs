using Microsoft.EntityFrameworkCore;
using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Admin;
using Shopbe.Domain.Entities.Category;
using Shopbe.Domain.Entities.Chatbot;
using Shopbe.Domain.Entities.Notification;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Payment;
using Shopbe.Domain.Entities.Product;
using Shopbe.Domain.Entities.Recommendation;
using Shopbe.Domain.Entities.Review;
using Shopbe.Domain.Entities.Shipping;
using Shopbe.Domain.Entities.ShoppingCart;
using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Entities.Wishlist;

namespace Shopbe.Infrastructure.Persistence;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    // DbSets
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<Brand> Brands { get; set; }
    public DbSet<ProductAttribute> Attributes { get; set; }
    public DbSet<AttributeValue> AttributeValues { get; set; }
    public DbSet<ProductVariantAttribute> ProductVariantAttributes { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }

    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<PaymentLog> PaymentLogs { get; set; }
    public DbSet<Refund> Refunds { get; set; }
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ShippingFee> ShippingFees { get; set; }

    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewImage> ReviewImages { get; set; }

    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<UserBehavior> UserBehaviors { get; set; }

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopDbContext).Assembly);
    }
}