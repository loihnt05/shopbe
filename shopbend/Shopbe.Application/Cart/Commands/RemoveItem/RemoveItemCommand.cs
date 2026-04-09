using MediatR;
using Shopbe.Application.Cart.Dtos;

namespace Shopbe.Application.Cart.Commands.RemoveItem;

public sealed record RemoveItemCommand(Guid UserId, Guid ProductVariantId) : IRequest<CartResponseDto>;

