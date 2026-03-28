namespace Shopbe.Domain.Entities.Shipping;

public class ShippingZoneDistrict : BaseEntity
{
    public Guid ZoneId { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;

    // Navigation Properties
    public ShippingZone? Zone { get; set; }
}

