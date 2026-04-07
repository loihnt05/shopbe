using Shopbe.Domain.Entities.User;
using Shopbe.Domain.Enums;

namespace Shopbe.Domain.Entities.Product;

public class InventoryTransaction : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public InventoryTransactionType Type { get; set; }
    public int Quantity { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Note { get; set; }
    public Guid? CreatedBy { get; set; }

    // Navigation Properties
    public ProductVariant? ProductVariant { get; set; }
    public User.User? User { get; set; }
}

