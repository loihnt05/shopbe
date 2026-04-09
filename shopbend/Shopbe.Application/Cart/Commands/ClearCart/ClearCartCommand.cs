using MediatR;
using Shopbe.Application.Cart.Dtos;

namespace Shopbe.Application.Cart.Commands.ClearCart;

public sealed record ClearCartCommand(Guid UserId) : IRequest<CartResponseDto>;

