namespace Shopbe.Domain.Entities.Product;

public class AttributeValue : BaseEntity
{
    public Guid AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    // Navigation Properties
    public ProductAttribute? Attribute { get; set; }
    public ICollection<ProductVariantAttribute> ProductVariantAttributes { get; set; } = new List<ProductVariantAttribute>();
}

