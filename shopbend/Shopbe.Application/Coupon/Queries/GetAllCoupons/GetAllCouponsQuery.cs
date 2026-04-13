using MediatR;
using Shopbe.Application.Coupon.Dtos;

namespace Shopbe.Application.Coupon.Queries.GetAllCoupons;

public record GetAllCouponsQuery(CouponQueryDto Filter) : IRequest<IEnumerable<CouponResponseDto>>;

