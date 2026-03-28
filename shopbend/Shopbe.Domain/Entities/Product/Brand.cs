namespace Shopbe.Domain.Entities.Product;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

