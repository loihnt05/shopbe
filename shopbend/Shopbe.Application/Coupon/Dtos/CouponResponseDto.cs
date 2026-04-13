using Shopbe.Domain.Enums;

namespace Shopbe.Application.Coupon.Dtos;

public record CouponResponseDto(
	Guid Id,
	string Code,
	string? Description,
	DiscountType DiscountType,
	decimal Value,
	decimal MinOrderAmount,
	decimal? MaxDiscountAmount,
	DateTime ExpiredAt,
	int? UsageLimit,
	int UsageCount,
	bool IsActive,
	DateTime CreatedAt,
	DateTime UpdatedAt
);
