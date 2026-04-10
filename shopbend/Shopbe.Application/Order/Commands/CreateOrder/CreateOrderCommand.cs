using MediatR;
using Shopbe.Application.Order.Dtos;

namespace Shopbe.Application.Order.Commands.CreateOrder;

public sealed record CreateOrderCommand(Guid UserId, CreateOrderRequestDto Request) : IRequest<OrderDetailsDto>;

