using MediatR;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Cart.Commands.AddItem;

public sealed class AddItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddItemCommand, CartResponseDto>
{
    public async Task<CartResponseDto> Handle(AddItemCommand request, CancellationToken cancellationToken)
    {
        var variant = await unitOfWork.ProductVariant.GetProductVariantByIdAsync(request.Request.ProductVariantId);
        if (variant is null)
            throw new KeyNotFoundException($"ProductVariant {request.Request.ProductVariantId} not found");

        await unitOfWork.Cart.AddOrIncrementItemAsync(
            request.UserId,
            request.Request.ProductVariantId,
            request.Request.Quantity,
            variant.Price,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var cart = await unitOfWork.Cart.GetOrCreateByUserIdAsync(request.UserId, cancellationToken);
        return cart.ToDto();
    }
}

