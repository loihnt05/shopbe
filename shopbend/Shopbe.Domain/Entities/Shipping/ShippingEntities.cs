namespace Shopbe.Domain.Entities.Shipping;

public class Shipment : BaseEntity
{
    public Guid OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public Guid? ShippingFeeId { get; set; }

    // Navigation properties
    public Order.Order? Order { get; set; }
    public ShippingFee? ShippingFee { get; set; }
}

public class ShippingFee : BaseEntity
{
    public string Region { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";

    // Navigation properties
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}

