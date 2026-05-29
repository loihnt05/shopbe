using Shopbe.Domain.Entities.Wishlist;

namespace Shopbe.Application.Common.Interfaces.IWishlist;

public interface IWishlistItemRepository
{
    Task<WishlistItem?> GetWishListItemByIdAsync(Guid id);
    Task CreateWishListItemAsync(WishlistItem wishlistItem);
    Task UpdateWishListItemAsync(WishlistItem wishlistItem);
    Task DeleteWishListItemAsync(Guid id);
    Task DeleteBulkAsync(Guid userId, IEnumerable<Guid> productIds);
    Task<IEnumerable<WishlistItem?>> GetWishlistItemByUserIdAsync(
        Guid userId,
        string? sortBy = null,
        bool? inStockOnly = null,
        int pageNumber = 1,
        int pageSize = 20);
}
