namespace Shopbe.Application.Shipping.Dtos;

public sealed class ShippingZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public IReadOnlyList<ShippingZoneDistrictDto> Districts { get; set; } = Array.Empty<ShippingZoneDistrictDto>();
}

