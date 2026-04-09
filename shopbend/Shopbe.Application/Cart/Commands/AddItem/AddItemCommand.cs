using MediatR;
using Shopbe.Application.Cart.Dtos;

namespace Shopbe.Application.Cart.Commands.AddItem;

public sealed record AddItemCommand(Guid UserId, AddCartItemRequestDto Request) : IRequest<CartResponseDto>;

