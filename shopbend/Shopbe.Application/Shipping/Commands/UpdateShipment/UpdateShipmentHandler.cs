using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Shipping.Dtos;

namespace Shopbe.Application.Shipping.Commands.UpdateShipment;

public sealed class UpdateShipmentCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateShipmentCommand, ShipmentDto?>
{
    public async Task<ShipmentDto?> Handle(UpdateShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await unitOfWork.Shipments.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment is null) return null;

        if (request.Request.Carrier is not null) shipment.Carrier = request.Request.Carrier.Trim();
        if (request.Request.TrackingNumber is not null) shipment.TrackingNumber = request.Request.TrackingNumber.Trim();
        if (request.Request.Status is not null) shipment.Status = request.Request.Status.Trim();

        shipment.ShippedAt = request.Request.ShippedAt ?? shipment.ShippedAt;
        shipment.DeliveredAt = request.Request.DeliveredAt ?? shipment.DeliveredAt;
        shipment.ShippingFeeId = request.Request.ShippingFeeId ?? shipment.ShippingFeeId;
        shipment.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ShipmentDto
        {
            Id = shipment.Id,
            OrderId = shipment.OrderId,
            Carrier = shipment.Carrier,
            TrackingNumber = shipment.TrackingNumber,
            Status = shipment.Status,
            ShippedAt = shipment.ShippedAt,
            DeliveredAt = shipment.DeliveredAt,
            ShippingFeeId = shipment.ShippingFeeId
        };
    }
}