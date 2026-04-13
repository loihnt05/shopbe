using AutoMapper;
using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Coupon.Dtos;
using Shopbe.Domain.Enums;

namespace Shopbe.Application.Coupon.Commands.UpdateCoupon;

public class UpdateCouponHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateCouponCommand, CouponResponseDto>
{
    public async Task<CouponResponseDto> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.Coupons.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (existing is null)
            throw new KeyNotFoundException("Coupon not found.");

        var r = request.Request;
        if (string.IsNullOrWhiteSpace(r.Code))
            throw new ArgumentException("Coupon code is required.");

        var code = r.Code.Trim();

        if (await unitOfWork.Coupons.ExistsByCodeAsync(code, request.Id, cancellationToken))
            throw new ArgumentException("Coupon code already exists.");

        ValidateCoupon(r);

        existing.Code = code;
        existing.Description = r.Description;
        existing.DiscountType = r.DiscountType;
        existing.Value = r.Value;
        existing.MinOrderAmount = r.MinOrderAmount;
        existing.MaxDiscountAmount = r.MaxDiscountAmount;
        existing.ExpiredAt = r.ExpiredAt;
        existing.UsageLimit = r.UsageLimit;
        existing.IsActive = r.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        // Tracked entity, so SaveChanges is enough; UpdateAsync kept for symmetry.
        await unitOfWork.Coupons.UpdateAsync(existing, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload as no-tracking for consistent DTO? Not necessary.
        return mapper.Map<CouponResponseDto>(existing);
    }

    private static void ValidateCoupon(CouponRequestDto r)
    {
        if (r.Value <= 0)
            throw new ArgumentException("Coupon value must be greater than 0.");

        if (r.MinOrderAmount < 0)
            throw new ArgumentException("MinOrderAmount must be >= 0.");

        if (r.MaxDiscountAmount.HasValue && r.MaxDiscountAmount.Value < 0)
            throw new ArgumentException("MaxDiscountAmount must be >= 0.");

        if (r.UsageLimit.HasValue && r.UsageLimit.Value <= 0)
            throw new ArgumentException("UsageLimit must be greater than 0 when specified.");

        if (r.DiscountType == DiscountType.Percentage)
        {
            if (r.Value > 100)
                throw new ArgumentException("Percentage coupon value must be between 0 and 100.");
        }
    }
}



