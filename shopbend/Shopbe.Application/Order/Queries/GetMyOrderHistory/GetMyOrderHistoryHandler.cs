using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Order.Dtos;

namespace Shopbe.Application.Order.Queries.GetMyOrderHistory;

public sealed class GetMyOrderHistoryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMyOrderHistoryQuery, IReadOnlyList<OrderStatusHistoryDto>>
{
    public async Task<IReadOnlyList<OrderStatusHistoryDto>> Handle(GetMyOrderHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdForUserAsync(request.OrderId, request.UserId, cancellationToken);
        if (order is null)
            throw new KeyNotFoundException("Order not found");

        return order.OrderStatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new OrderStatusHistoryDto
            {
                Status = h.Status,
                Note = h.Note,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt
            })
            .ToList();
    }
}

