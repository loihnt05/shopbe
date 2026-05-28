using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Coupon.Dtos;
using Shopbe.Application.Coupon;

namespace Shopbe.Application.Coupon.Queries.GetCouponByCode;

public class GetCouponByCodeHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCouponByCodeQuery, CouponResponseDto?>
{
    public async Task<CouponResponseDto?> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
    {
        var coupon = await unitOfWork.Coupons.GetByCodeAsync(request.Code, cancellationToken);
        return coupon?.ToDto();
    }
}


