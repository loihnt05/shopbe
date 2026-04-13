using MediatR;
using Shopbe.Application.Coupon.Dtos;

namespace Shopbe.Application.Coupon.Commands.CreateCoupon;

public record CreateCouponCommand(CouponRequestDto Request) : IRequest<CouponResponseDto>;

