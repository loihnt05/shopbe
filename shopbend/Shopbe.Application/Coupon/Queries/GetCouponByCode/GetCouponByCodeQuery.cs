using MediatR;
using Shopbe.Application.Coupon.Dtos;

namespace Shopbe.Application.Coupon.Queries.GetCouponByCode;

public record GetCouponByCodeQuery(string Code) : IRequest<CouponResponseDto?>;

