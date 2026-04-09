using MediatR;
using Shopbe.Application.Cart.Dtos;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Cart.Queries.GetMyCart;

public sealed class GetMyCartHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMyCartQuery, CartResponseDto>
{
    public async Task<CartResponseDto> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await unitOfWork.Cart.GetOrCreateByUserIdAsync(request.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return cart.ToDto();
    }
}

