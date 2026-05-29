using MediatR;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Application.Wishlist.Commands.BulkRemoveWishlistItems;

public sealed class BulkRemoveWishlistItemsHandler(IUnitOfWork unitOfWork) : IRequestHandler<BulkRemoveWishlistItemsCommand, bool>
{
    public async Task<bool> Handle(BulkRemoveWishlistItemsCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.WishlistItems.DeleteBulkAsync(request.UserId, request.ProductIds);
        return true;
    }
}
