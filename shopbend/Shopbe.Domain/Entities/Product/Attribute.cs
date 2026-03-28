namespace Shopbe.Domain.Entities.Product;

public class Attribute : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
}

