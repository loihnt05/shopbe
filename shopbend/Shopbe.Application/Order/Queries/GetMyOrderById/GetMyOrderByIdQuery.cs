using MediatR;
using Shopbe.Application.Order.Dtos;

namespace Shopbe.Application.Order.Queries.GetMyOrderById;

public sealed record GetMyOrderByIdQuery(Guid UserId, Guid OrderId) : IRequest<OrderDetailsDto?>;

