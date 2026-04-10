using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Order.Dtos;

namespace Shopbe.Application.Order.Queries.GetMyOrderById;

public sealed class GetMyOrderByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyOrderByIdQuery, OrderDetailsDto?>
{
    public async Task<OrderDetailsDto?> Handle(GetMyOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdForUserAsync(request.OrderId, request.UserId, cancellationToken);
        return order?.ToDetailsDto();
    }
}

