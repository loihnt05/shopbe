using Shopbe.Domain.Entities;

namespace Shopbe.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid ProductId { get; set; }
    
    // Navigation Property
    public Product? Product { get; set; }
}