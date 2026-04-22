using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Wishlist.Commands.RemoveWishlistItem;

public sealed class RemoveWishlistItemHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveWishlistItemCommand, bool>
{
    public async Task<bool> Handle(RemoveWishlistItemCommand request, CancellationToken cancellationToken)
    {
        var items = await unitOfWork.WishlistItems.GetWishlistItemByUserIdAsync(request.UserId);
        var item = items.FirstOrDefault(i => i is not null && i.ProductId == request.ProductId);
        if (item is null)
            return false;

        await unitOfWork.WishlistItems.DeleteWishListItemAsync(item.Id);
        return true;
    }
}

