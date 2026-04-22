using Shopbe.Application.Wishlist.Dtos;
using Shopbe.Domain.Entities.Wishlist;

namespace Shopbe.Application.Wishlist;

public static class WishlistMapping
{
    public static WishlistItemResponseDto ToDto(this WishlistItem item)
        => new(item.Id, item.UserId, item.ProductId);
}

