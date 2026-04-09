using MediatR;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Cart.Commands.ClearCart;

public sealed class ClearCartHandler(IUnitOfWork unitOfWork) : IRequestHandler<ClearCartCommand, CartResponseDto>
{
    public async Task<CartResponseDto> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.Cart.ClearAsync(request.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var cart = await unitOfWork.Cart.GetOrCreateByUserIdAsync(request.UserId, cancellationToken);
        return cart.ToDto();
    }
}

