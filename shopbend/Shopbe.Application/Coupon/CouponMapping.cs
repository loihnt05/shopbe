using Shopbe.Application.Coupon.Dtos;
using CouponEntity = Shopbe.Domain.Entities.Order.Coupon;

namespace Shopbe.Application.Coupon;

public static class CouponMapping
{
    public static CouponResponseDto ToDto(this CouponEntity coupon)
        => new(
            coupon.Id,
            coupon.Code,
            coupon.Description,
            coupon.DiscountType,
            coupon.Value,
            coupon.MinOrderAmount,
            coupon.MaxDiscountAmount,
            coupon.ExpiredAt,
            coupon.UsageLimit,
            coupon.UsageCount,
            coupon.IsActive,
            coupon.CreatedAt,
            coupon.UpdatedAt);
}


