namespace Shopbe.Application.Shipping.Dtos;

public sealed class ShipmentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public Guid? ShippingFeeId { get; set; }
}

public sealed class CreateShipmentRequestDto
{
    public Guid OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Created";
    public DateTime? ShippedAt { get; set; }
    public Guid? ShippingFeeId { get; set; }
}

public sealed class UpdateShipmentRequestDto
{
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Status { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public Guid? ShippingFeeId { get; set; }
}

