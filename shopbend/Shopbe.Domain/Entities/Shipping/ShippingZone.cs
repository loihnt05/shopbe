namespace Shopbe.Domain.Entities.Shipping;

public class ShippingZone : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Fee { get; set; }

    // Navigation Properties
    public ICollection<ShippingZoneDistrict> ShippingZoneDistricts { get; set; } = new List<ShippingZoneDistrict>();
}

