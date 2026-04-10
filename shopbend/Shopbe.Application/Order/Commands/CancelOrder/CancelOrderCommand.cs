using MediatR;
using Shopbe.Application.Order.Dtos;

namespace Shopbe.Application.Order.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid UserId, Guid OrderId, string? Reason) : IRequest<OrderDetailsDto>;

