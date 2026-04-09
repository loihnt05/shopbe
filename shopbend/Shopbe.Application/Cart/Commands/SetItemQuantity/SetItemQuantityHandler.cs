using MediatR;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Cart.Commands.SetItemQuantity;

public sealed class SetItemQuantityHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SetItemQuantityCommand, CartResponseDto>
{
    public async Task<CartResponseDto> Handle(SetItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var updated = await unitOfWork.Cart.SetItemQuantityAsync(
            request.UserId,
            request.ProductVariantId,
            request.Request.Quantity,
            cancellationToken);

        if (updated is null)
            throw new KeyNotFoundException("Cart item not found");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var cart = await unitOfWork.Cart.GetOrCreateByUserIdAsync(request.UserId, cancellationToken);
        return cart.ToDto();
    }
}

