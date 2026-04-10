using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Queries.GetShipmentsByOrderId;

public sealed class GetShipmentsByOrderIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetShipmentsByOrderIdQuery, IReadOnlyList<ShipmentDto>>
{
    public async Task<IReadOnlyList<ShipmentDto>> Handle(GetShipmentsByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var shipments = await unitOfWork.Shipments.ListByOrderIdAsync(request.OrderId, cancellationToken);

        return shipments.Select(s => new ShipmentDto
        {
            Id = s.Id,
            OrderId = s.OrderId,
            Carrier = s.Carrier,
            TrackingNumber = s.TrackingNumber,
            Status = s.Status,
            ShippedAt = s.ShippedAt,
            DeliveredAt = s.DeliveredAt,
            ShippingFeeId = s.ShippingFeeId
        }).ToList();
    }
}