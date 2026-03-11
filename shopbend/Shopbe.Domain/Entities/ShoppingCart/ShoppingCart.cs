namespace Shopbe.Domain.Entities;

public class ShoppingCart : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Navigation Properties
    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}