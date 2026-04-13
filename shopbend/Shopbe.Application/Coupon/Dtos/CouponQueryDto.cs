namespace Shopbe.Application.Coupon.Dtos;

public record CouponQueryDto(
	string? Code = null,
	bool? IsActive = null,
	bool? IsExpired = null,
	int? Skip = null,
	int? Take = null
);
