using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Application.Common.Interfaces.IShipping;

public interface IShipmentRepository
{
    Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shipment>> ListByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}

