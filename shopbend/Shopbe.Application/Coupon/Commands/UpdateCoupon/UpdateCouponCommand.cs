using MediatR;
using Shopbe.Application.Coupon.Dtos;

namespace Shopbe.Application.Coupon.Commands.UpdateCoupon;

public record UpdateCouponCommand(Guid Id, CouponRequestDto Request) : IRequest<CouponResponseDto>;

