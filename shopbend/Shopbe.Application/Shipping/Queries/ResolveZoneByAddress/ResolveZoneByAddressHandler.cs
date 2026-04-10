using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Queries.ResolveZoneByAddress;

public sealed class ResolveZoneByAddressQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ResolveZoneByAddressQuery, ShippingZoneDto?>
{
    public async Task<ShippingZoneDto?> Handle(ResolveZoneByAddressQuery request, CancellationToken cancellationToken)
    {
        var city = request.City.Trim();
        var district = request.District.Trim();

        var mapping = await unitOfWork.ShippingZoneDistricts.FindByCityDistrictAsync(city, district, cancellationToken);
        if (mapping is null)
            return null;

        var zone = await unitOfWork.ShippingZones.GetByIdAsync(mapping.ZoneId, includeDistricts: false, cancellationToken);
        if (zone is null)
            return null;

        return new ShippingZoneDto { Id = zone.Id, Name = zone.Name, Fee = zone.Fee, Districts = Array.Empty<ShippingZoneDistrictDto>() };
    }
}