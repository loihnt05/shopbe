using MediatR;
using Shopbe.Application.Cart.Dtos;

namespace Shopbe.Application.Cart.Commands.RemoveCoupon;

public record RemoveCouponCommand() : IRequest<CartResponseDto>;
