using MediatR;
using Shopbe.Application.Order.Dtos;

namespace Shopbe.Application.Order.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResultDto<OrderSummaryDto>>;

