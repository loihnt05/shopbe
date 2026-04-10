using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Application.Common.Interfaces.IShipping;

public interface IShippingZoneDistrictRepository
{
    Task<bool> ExistsAsync(Guid zoneId, string city, string district, CancellationToken cancellationToken = default);
    Task AddAsync(ShippingZoneDistrict district, CancellationToken cancellationToken = default);
    Task<ShippingZoneDistrict?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Remove(ShippingZoneDistrict district);
    Task<ShippingZoneDistrict?> FindByCityDistrictAsync(string city, string district, CancellationToken cancellationToken = default);
}

