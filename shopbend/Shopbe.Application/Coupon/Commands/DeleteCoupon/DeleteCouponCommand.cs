using MediatR;

namespace Shopbe.Application.Coupon.Commands.DeleteCoupon;

public record DeleteCouponCommand(Guid Id) : IRequest;

