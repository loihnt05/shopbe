using MediatR;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Cart.Commands.RemoveCoupon;

public class RemoveCouponHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser) 
    : IRequestHandler<RemoveCouponCommand, CartResponseDto>
{
    public async Task<CartResponseDto> Handle(RemoveCouponCommand request, CancellationToken cancellationToken)
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw new UnauthorizedAccessException();
        }

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId)
            ?? throw new UnauthorizedAccessException();

        var cart = await unitOfWork.Cart.GetOrCreateByUserIdAsync(user.Id, cancellationToken);
        
        cart.CouponId = null;
        cart.Coupon = null;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return cart.ToDto();
    }
}
