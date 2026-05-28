using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Coupon.Dtos;
using Shopbe.Application.Coupon;

namespace Shopbe.Application.Coupon.Queries.GetCouponById;

public class GetCouponByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCouponByIdQuery, CouponResponseDto?>
{
    public async Task<CouponResponseDto?> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = await unitOfWork.Coupons.GetByIdAsync(request.Id, cancellationToken);
        return coupon?.ToDto();
    }
}


