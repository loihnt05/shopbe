namespace Shopbe.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class ProductAttribute : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
}

public class AttributeValue : BaseEntity
{
    public Guid ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    // Navigation properties
    public ProductAttribute? ProductAttribute { get; set; }
    public ICollection<ProductVariantAttribute> ProductVariantAttributes { get; set; } = new List<ProductVariantAttribute>();
}

public class ProductVariantAttribute : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public Guid AttributeValueId { get; set; }

    // Navigation properties
    public ProductVariant? ProductVariant { get; set; }
    public AttributeValue? AttributeValue { get; set; }
}

public class InventoryTransaction : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int QuantityChanged { get; set; }
    public string? Reason { get; set; }

    // Navigation properties
    public ProductVariant? ProductVariant { get; set; }
}

