using Shopbe.Domain.Enums;

namespace Shopbe.Application.Coupon.Dtos;

public record CouponRequestDto(
	string Code,
	string? Description,
	DiscountType DiscountType,
	decimal Value,
	decimal MinOrderAmount,
	decimal? MaxDiscountAmount,
	DateTime ExpiredAt,
	int? UsageLimit,
	bool IsActive
);
