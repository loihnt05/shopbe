using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Coupon.Dtos;

namespace Shopbe.Application.Coupon.Queries.GetCouponById;

public class GetCouponByIdHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetCouponByIdQuery, CouponResponseDto?>
{
    public async Task<CouponResponseDto?> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = await unitOfWork.Coupons.GetByIdAsync(request.Id, cancellationToken);
        return coupon is null ? null : mapper.Map<CouponResponseDto>(coupon);
    }
}


