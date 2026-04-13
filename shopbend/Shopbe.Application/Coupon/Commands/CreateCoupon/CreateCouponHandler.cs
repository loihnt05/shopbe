using MediatR;
using AutoMapper;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Coupon.Dtos;
using CouponEntity = Shopbe.Domain.Entities.Order.Coupon;
using Shopbe.Domain.Enums;

namespace Shopbe.Application.Coupon.Commands.CreateCoupon;

public class CreateCouponHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateCouponCommand, CouponResponseDto>
{
    public async Task<CouponResponseDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var r = request.Request;

        if (string.IsNullOrWhiteSpace(r.Code))
            throw new ArgumentException("Coupon code is required.");

        var code = r.Code.Trim();

        if (await unitOfWork.Coupons.ExistsByCodeAsync(code, null, cancellationToken))
            throw new ArgumentException("Coupon code already exists.");

        ValidateCoupon(r);

        var now = DateTime.UtcNow;
        var coupon = new CouponEntity
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = r.Description,
            DiscountType = r.DiscountType,
            Value = r.Value,
            MinOrderAmount = r.MinOrderAmount,
            MaxDiscountAmount = r.MaxDiscountAmount,
            ExpiredAt = r.ExpiredAt,
            UsageLimit = r.UsageLimit,
            UsageCount = 0,
            IsActive = r.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await unitOfWork.Coupons.AddAsync(coupon, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CouponResponseDto>(coupon);
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



