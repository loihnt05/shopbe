using Shopbe.Domain.Entities;

namespace Shopbe.Domain.Entities;

public class ProductImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public bool IsPrimary { get; set; }
    
    // Navigation Property
    public Product? Product { get; set; }
}