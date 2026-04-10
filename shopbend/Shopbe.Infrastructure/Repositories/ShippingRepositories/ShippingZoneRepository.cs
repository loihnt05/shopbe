using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IShipping;
using Shopbe.Domain.Entities.Shipping;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ShippingRepositories;

public sealed class ShippingZoneRepository(ShopDbContext context) : IShippingZoneRepository
{
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var norm = name.Trim();
        return context.ShippingZones.AnyAsync(z => z.Name == norm, cancellationToken);
    }

    public async Task AddAsync(ShippingZone zone, CancellationToken cancellationToken = default)
    {
        await context.ShippingZones.AddAsync(zone, cancellationToken);
    }

    public async Task<ShippingZone?> GetByIdAsync(Guid id, bool includeDistricts, CancellationToken cancellationToken = default)
    {
        var query = context.ShippingZones.AsQueryable();
        if (includeDistricts)
            query = query.Include(z => z.ShippingZoneDistricts);
        return await query.FirstOrDefaultAsync(z => z.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ShippingZone>> ListAsync(bool includeDistricts, CancellationToken cancellationToken = default)
    {
        var query = context.ShippingZones.AsNoTracking().AsQueryable();
        if (includeDistricts)
            query = query.Include(z => z.ShippingZoneDistricts);
        return await query.OrderBy(z => z.Name).ToListAsync(cancellationToken);
    }
}

