using MediatR;
using Shopbe.Application.Cart.Dtos;

namespace Shopbe.Application.Cart.Queries.GetMyCart;

public sealed record GetMyCartQuery(Guid UserId) : IRequest<CartResponseDto>;

