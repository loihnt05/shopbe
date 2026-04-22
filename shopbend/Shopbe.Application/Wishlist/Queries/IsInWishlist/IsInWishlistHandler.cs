using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Wishlist.Queries.IsInWishlist;

public sealed class IsInWishlistHandler(IUnitOfWork unitOfWork) : IRequestHandler<IsInWishlistQuery, bool>
{
    public async Task<bool> Handle(IsInWishlistQuery request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.WishlistItems.GetWishlistItemByUserIdAsync(request.UserId);
        return items.Any(i => i is not null && i.ProductId == request.ProductId);
    }
}

