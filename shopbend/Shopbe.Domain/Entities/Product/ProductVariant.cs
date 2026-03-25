using Shopbe.Domain.Entities;

namespace Shopbe.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid ProductId { get; set; }
    
    // Navigation Properties
    public Product? Product { get; set; }
    public ICollection<ProductVariantAttribute> ProductVariantAttributes { get; set; } = new List<ProductVariantAttribute>();
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
}