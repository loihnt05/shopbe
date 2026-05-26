using MediatR;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Enums;
using CouponEntity = Shopbe.Domain.Entities.Order.Coupon;

namespace Shopbe.Application.Cart.Commands.ApplyCoupon;

public class ApplyCouponHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser) 
    : IRequestHandler<ApplyCouponCommand, CartResponseDto>
{
    public async Task<CartResponseDto> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw new UnauthorizedAccessException();
        }

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId)
            ?? throw new UnauthorizedAccessException();

        var cart = await unitOfWork.Cart.GetOrCreateByUserIdAsync(user.Id, cancellationToken);
        
        var coupon = await unitOfWork.Coupons.GetByCodeAsync(request.CouponCode, cancellationToken)
            ?? throw new KeyNotFoundException($"Coupon with code '{request.CouponCode}' not found.");

        if (!coupon.IsActive)
        {
            throw new InvalidOperationException("Coupon is not active.");
        }

        if (coupon.ExpiredAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Coupon has expired.");
        }

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
        {
            throw new InvalidOperationException("Coupon usage limit reached.");
        }

        // Calculate subtotal to check MinOrderAmount
        var items = cart.CartItems;
        var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);

        if (subtotal < coupon.MinOrderAmount)
        {
            throw new InvalidOperationException($"Minimum order amount for this coupon is {coupon.MinOrderAmount}.");
        }

        cart.CouponId = coupon.Id;
        cart.Coupon = coupon; // Attach for ToDto mapping

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return cart.ToDto();
    }
}
