using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Queries.GetShippingZoneById;

public sealed class GetShippingZoneByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetShippingZoneByIdQuery, ShippingZoneDto?>
{
    public async Task<ShippingZoneDto?> Handle(GetShippingZoneByIdQuery request, CancellationToken cancellationToken)
    {
        var z = await unitOfWork.ShippingZones.GetByIdAsync(request.Id, request.IncludeDistricts, cancellationToken);
        if (z is null) return null;

        return new ShippingZoneDto
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
        };
    }
}