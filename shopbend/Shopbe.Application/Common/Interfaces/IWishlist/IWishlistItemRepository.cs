using Shopbe.Domain.Entities.Wishlist;

namespace Shopbe.Application.Common.Interfaces.IWishlist;

public interface IWishlistItemRepository
{
    Task<WishlistItem?> GetWishListItemByIdAsync(Guid id);
    Task CreateWishListItemAsync(WishlistItem wishlistItem);
    Task UpdateWishListItemAsync(WishlistItem wishlistItem);
    Task DeleteWishListItemAsync(Guid id);
    Task<IEnumerable<WishlistItem?>> GetWishlistItemByUserIdAsync(Guid userId);
}