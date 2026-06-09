using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.Notifications;
using Shopbe.Application.Shipping.Dtos;
using Shopbe.Domain.Entities.Shipping;

namespace Shopbe.Application.Shipping.Commands.CreateShipment;

public sealed class CreateShipmentCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService) : IRequestHandler<CreateShipmentCommand, ShipmentDto>
{
    public async Task<ShipmentDto> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        // Order repository doesn't expose an Exists() in current codebase; using GetByIdAsync for existence check.
        var order = await unitOfWork.Orders.GetByIdAsync(request.Request.OrderId, cancellationToken);
        var orderExists = order is not null;
        if (!orderExists)
            throw new KeyNotFoundException("Order not found");

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            OrderId = request.Request.OrderId,
            Carrier = request.Request.Carrier.Trim(),
            TrackingNumber = request.Request.TrackingNumber.Trim(),
            Status = request.Request.Status.Trim(),
            ShippedAt = request.Request.ShippedAt,
            DeliveredAt = null,
            ShippingFeeId = request.Request.ShippingFeeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await unitOfWork.Shipments.AddAsync(shipment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await SendShipmentNotificationAsync(shipment.OrderId, shipment.Status, shipment.Carrier, shipment.TrackingNumber, notificationService, cancellationToken);

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

    private static Task SendShipmentNotificationAsync(
        Guid orderId,
        string status,
        string carrier,
        string trackingNumber,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        if (status.Contains("delivered", StringComparison.OrdinalIgnoreCase))
        {
            return notificationService.SendOrderDeliveredAsync(orderId, carrier, trackingNumber, cancellationToken);
        }

        if (status.Contains("shipped", StringComparison.OrdinalIgnoreCase))
        {
            return notificationService.SendOrderShippedAsync(orderId, carrier, trackingNumber, cancellationToken);
        }

        return Task.CompletedTask;
    }
}
