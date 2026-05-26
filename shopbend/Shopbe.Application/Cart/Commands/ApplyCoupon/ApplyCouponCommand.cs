using MediatR;
using Shopbe.Application.Cart.Dtos;

namespace Shopbe.Application.Cart.Commands.ApplyCoupon;

public record ApplyCouponCommand(string CouponCode) : IRequest<CartResponseDto>;
