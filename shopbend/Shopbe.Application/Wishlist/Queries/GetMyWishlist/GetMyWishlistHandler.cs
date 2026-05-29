using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Wishlist.Dtos;

namespace Shopbe.Application.Wishlist.Queries.GetMyWishlist;

public sealed class GetMyWishlistHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMyWishlistQuery, IReadOnlyList<WishlistItemResponseDto>>
{
    public async Task<IReadOnlyList<WishlistItemResponseDto>> Handle(GetMyWishlistQuery request,
        CancellationToken cancellationToken)
    {
        var items = await unitOfWork.WishlistItems.GetWishlistItemByUserIdAsync(
            request.UserId,
            request.Query.SortBy,
            request.Query.InStockOnly,
            request.Query.PageNumber,
            request.Query.PageSize);
            
        return items
            .Where(i => i is not null)
            .Select(i => i!.ToDto())
            .ToList();
    }
}
