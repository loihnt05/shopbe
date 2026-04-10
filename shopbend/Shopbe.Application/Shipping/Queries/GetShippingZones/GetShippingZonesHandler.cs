using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Queries.GetShippingZones;

public sealed class GetShippingZonesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetShippingZonesQuery, IReadOnlyList<ShippingZoneDto>>
{
    public async Task<IReadOnlyList<ShippingZoneDto>> Handle(GetShippingZonesQuery request, CancellationToken cancellationToken)
    {
        var zones = await unitOfWork.ShippingZones.ListAsync(request.IncludeDistricts, cancellationToken);

        return zones.Select(z => new ShippingZoneDto
        {
            Id = z.Id,
            Name = z.Name,
            Fee = z.Fee,
            Districts = request.IncludeDistricts
                ? z.ShippingZoneDistricts.Select(d => new ShippingZoneDistrictDto
                {
                    Id = d.Id,
                    ZoneId = d.ZoneId,
                    City = d.City,
                    District = d.District
                }).ToList()
                : Array.Empty<ShippingZoneDistrictDto>()
        }).ToList();
    }
}