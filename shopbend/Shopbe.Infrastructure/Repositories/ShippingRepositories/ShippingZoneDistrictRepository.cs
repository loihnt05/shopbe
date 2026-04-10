using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IShipping;
using Shopbe.Domain.Entities.Shipping;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ShippingRepositories;

public sealed class ShippingZoneDistrictRepository(ShopDbContext context) : IShippingZoneDistrictRepository
{
    public Task<bool> ExistsAsync(Guid zoneId, string city, string district, CancellationToken cancellationToken = default)
    {
        var c = city.Trim();
        var d = district.Trim();
        return context.ShippingZoneDistricts.AnyAsync(x => x.ZoneId == zoneId && x.City == c && x.District == d, cancellationToken);
    }

    public async Task AddAsync(ShippingZoneDistrict district, CancellationToken cancellationToken = default)
    {
        await context.ShippingZoneDistricts.AddAsync(district, cancellationToken);
    }

    public Task<ShippingZoneDistrict?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.ShippingZoneDistricts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Remove(ShippingZoneDistrict district)
    {
        context.ShippingZoneDistricts.Remove(district);
    }

    public Task<ShippingZoneDistrict?> FindByCityDistrictAsync(string city, string district, CancellationToken cancellationToken = default)
    {
        var c = city.Trim();
        var d = district.Trim();
        return context.ShippingZoneDistricts.AsNoTracking().FirstOrDefaultAsync(x => x.City == c && x.District == d, cancellationToken);
    }
}

