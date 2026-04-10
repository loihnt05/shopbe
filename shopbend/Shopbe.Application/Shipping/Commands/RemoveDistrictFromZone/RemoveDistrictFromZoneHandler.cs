using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Shipping.Commands.RemoveDistrictFromZone;

public sealed class RemoveDistrictFromZoneCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveDistrictFromZoneCommand, bool>
{
    public async Task<bool> Handle(RemoveDistrictFromZoneCommand request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.ShippingZoneDistricts.GetByIdAsync(request.DistrictId, cancellationToken);
        if (entity is not null && entity.ZoneId != request.ZoneId)
            return false;
        if (entity is null)
            return false;

        unitOfWork.ShippingZoneDistricts.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}