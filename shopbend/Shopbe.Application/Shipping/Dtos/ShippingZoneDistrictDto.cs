namespace Shopbe.Application.Shipping.Dtos;

public sealed class ShippingZoneDistrictDto
{
    public Guid Id { get; set; }
    public Guid ZoneId { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
}

