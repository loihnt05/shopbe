using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Order.Dtos;

namespace Shopbe.Application.Order.Queries.GetMyOrders;

public sealed class GetMyOrdersHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyOrdersQuery, PagedResultDto<OrderSummaryDto>>
{
    public async Task<PagedResultDto<OrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await unitOfWork.Orders.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
        var total = await unitOfWork.Orders.CountByUserIdAsync(request.UserId, cancellationToken);

        return new PagedResultDto<OrderSummaryDto>
        {
            Items = orders.Select(o => o.ToSummaryDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        };
    }
}

