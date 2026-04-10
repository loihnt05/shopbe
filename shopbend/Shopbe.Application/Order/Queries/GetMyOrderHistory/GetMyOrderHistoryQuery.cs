using MediatR;
using Shopbe.Application.Order.Dtos;

namespace Shopbe.Application.Order.Queries.GetMyOrderHistory;

public sealed record GetMyOrderHistoryQuery(Guid UserId, Guid OrderId) : IRequest<IReadOnlyList<OrderStatusHistoryDto>>;

