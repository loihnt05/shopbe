using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Application.Shipping.Commands.CreateShippingZone;

public sealed class CreateShippingZoneCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateShippingZoneCommand, ShippingZoneDto>
{
    public async Task<ShippingZoneDto> Handle(CreateShippingZoneCommand request, CancellationToken cancellationToken)
    {
        var exists = await unitOfWork.ShippingZones.ExistsByNameAsync(request.Name.Trim(), cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Shipping zone '{request.Name}' already exists");

        var zone = new ShippingZone
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Fee = request.Fee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await unitOfWork.ShippingZones.AddAsync(zone, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ShippingZoneDto
        {
            Id = zone.Id,
            Name = zone.Name,
            Fee = zone.Fee,
            Districts = Array.Empty<ShippingZoneDistrictDto>()
        };
    }
}