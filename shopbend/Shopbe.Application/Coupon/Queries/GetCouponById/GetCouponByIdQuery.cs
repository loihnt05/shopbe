using MediatR;
using Shopbe.Application.Coupon.Dtos;

namespace Shopbe.Application.Coupon.Queries.GetCouponById;

public record GetCouponByIdQuery(Guid Id) : IRequest<CouponResponseDto?>;

