namespace Shopbe.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid ShoppingCartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}