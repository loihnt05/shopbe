namespace Shopbe.Domain.Entities.Product;

public class ProductVariantAttribute
{
    public Guid VariantId { get; set; }
    public Guid AttributeValueId { get; set; }

    // Navigation Properties
    public ProductVariant? Variant { get; set; }
    public AttributeValue? AttributeValue { get; set; }
}

