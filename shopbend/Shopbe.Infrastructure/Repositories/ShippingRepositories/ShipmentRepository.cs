using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IShipping;
using Shopbe.Domain.Entities.Shipping;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ShippingRepositories;

public sealed class ShipmentRepository(ShopDbContext context) : IShipmentRepository
{
    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        await context.Shipments.AddAsync(shipment, cancellationToken);
    }

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Shipments.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Shipment>> ListByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await context.Shipments.AsNoTracking()
            .Where(s => s.OrderId == orderId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

