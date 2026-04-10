using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Order.Dtos;
using Shopbe.Domain.Entities.Order;
using Shopbe.Domain.Enums;

namespace Shopbe.Application.Order.Commands.CancelOrder;

public sealed class CancelOrderHandler(IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, OrderDetailsDto>
{
    public async Task<OrderDetailsDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetTrackedByIdForUserAsync(request.OrderId, request.UserId, cancellationToken);
        if (order is null)
            throw new KeyNotFoundException("Order not found");

        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded)
            throw new InvalidOperationException("Order status '" + order.Status + "' cannot be cancelled");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        order.OrderStatusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = OrderStatus.Cancelled,
            ChangedBy = request.UserId,
            ChangedAt = DateTime.UtcNow,
            Note = string.IsNullOrWhiteSpace(request.Reason) ? "Cancelled by user" : request.Reason
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.ToDetailsDto();
    }
}



