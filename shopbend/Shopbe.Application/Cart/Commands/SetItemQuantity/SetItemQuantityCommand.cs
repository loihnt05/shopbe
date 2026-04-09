using MediatR;
using Shopbe.Application.Cart.Dtos;

namespace Shopbe.Application.Cart.Commands.SetItemQuantity;

public sealed record SetItemQuantityCommand(Guid UserId, Guid ProductVariantId, SetCartItemQuantityRequestDto Request)
    : IRequest<CartResponseDto>;

