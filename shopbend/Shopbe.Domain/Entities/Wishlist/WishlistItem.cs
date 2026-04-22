namespace Shopbe.Domain.Entities.Wishlist;

public class WishlistItem : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }

    // Navigation properties
    public User.User? User { get; set; }
    public Product.Product? Product { get; set; }
}