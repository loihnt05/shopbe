using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Application.Common.Interfaces.IShipping;

public interface IShippingZoneRepository
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(ShippingZone zone, CancellationToken cancellationToken = default);
    Task<ShippingZone?> GetByIdAsync(Guid id, bool includeDistricts, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShippingZone>> ListAsync(bool includeDistricts, CancellationToken cancellationToken = default);
}

