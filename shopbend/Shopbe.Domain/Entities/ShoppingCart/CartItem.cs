namespace Shopbe.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid ShoppingCartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    
    // Navigation Properties
    public ShoppingCart? ShoppingCart { get; set; }
    public Product? Product { get; set; }
}