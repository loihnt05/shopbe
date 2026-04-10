using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Application.Shipping.Commands.AddDistrictToZone;

public sealed class AddDistrictToZoneCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AddDistrictToZoneCommand, ShippingZoneDistrictDto>
{
    public async Task<ShippingZoneDistrictDto> Handle(AddDistrictToZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await unitOfWork.ShippingZones.GetByIdAsync(request.ZoneId, includeDistricts: false, cancellationToken);
        if (zone is null)
            throw new KeyNotFoundException("Shipping zone not found");

        var city = request.City.Trim();
        var district = request.District.Trim();

        var dup = await unitOfWork.ShippingZoneDistricts.ExistsAsync(request.ZoneId, city, district, cancellationToken);
        if (dup)
            throw new InvalidOperationException("District already assigned to this zone");

        var entity = new ShippingZoneDistrict
        {
            Id = Guid.NewGuid(),
            ZoneId = request.ZoneId,
            City = city,
            District = district,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await unitOfWork.ShippingZoneDistricts.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ShippingZoneDistrictDto
        {
            Id = entity.Id,
            ZoneId = entity.ZoneId,
            City = entity.City,
            District = entity.District
        };
    }
}
