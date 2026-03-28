using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Entities.Recommendation;
using Shopbe.Domain.Entities.ShoppingCart;
using Shopbe.Domain.Entities.Wishlist;

namespace Shopbe.Domain.Entities.Product;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public Category.Category? Category { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Review.Review> Reviews { get; set; } = new List<Review.Review>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<UserBehavior> UserBehaviors { get; set; } = new List<UserBehavior>();
}