using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Coupon.Commands.DeleteCoupon;

public class DeleteCouponHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteCouponCommand>
{
    public async Task Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.Coupons.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
            throw new KeyNotFoundException("Coupon not found.");

        await unitOfWork.Coupons.DeleteAsync(request.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

