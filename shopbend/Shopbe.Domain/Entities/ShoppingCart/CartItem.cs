namespace Shopbe.Domain.Entities.ShoppingCart;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation Properties
    public ShoppingCart? Cart { get; set; }
    public Product.ProductVariant? ProductVariant { get; set; }
}