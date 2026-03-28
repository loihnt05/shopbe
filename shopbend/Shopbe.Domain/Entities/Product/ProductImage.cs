namespace Shopbe.Domain.Entities.Product;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    
    // Navigation Property
    public Product? Product { get; set; }
}