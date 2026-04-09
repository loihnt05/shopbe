using MediatR;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Cart.Commands.RemoveItem;

public sealed class RemoveItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveItemCommand, CartResponseDto>
{
    public async Task<CartResponseDto> Handle(RemoveItemCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.Cart.RemoveItemAsync(request.UserId, request.ProductVariantId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var cart = await unitOfWork.Cart.GetOrCreateByUserIdAsync(request.UserId, cancellationToken);
        return cart.ToDto();
    }
}

