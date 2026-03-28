namespace Shopbe.Domain.Entities.ShoppingCart;

public class ShoppingCart : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public User.User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}