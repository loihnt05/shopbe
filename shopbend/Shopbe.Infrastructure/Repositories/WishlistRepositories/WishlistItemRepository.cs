using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IWishlist;
using Shopbe.Domain.Entities.Wishlist;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.WishlistRepositories;

public class WishlistItemRepository(ShopDbContext context) : IWishlistItemRepository
{
    public async Task<WishlistItem?> GetWishListItemByIdAsync(Guid id)
    {
        return await context.WishlistItems.FindAsync(id);
    }

    public async Task CreateWishListItemAsync(WishlistItem wishlistItem)
    {
        await context.WishlistItems.AddAsync(wishlistItem);
        await context.SaveChangesAsync();
    }

    public async Task UpdateWishListItemAsync(WishlistItem wishlistItem)
    {
        context.WishlistItems.Update(wishlistItem);
        await context.SaveChangesAsync();
    }

    public async Task DeleteWishListItemAsync(Guid id)
    {
        var wishlistItem = await context.WishlistItems.FindAsync(id);
        if (wishlistItem != null)
        {
            context.WishlistItems.Remove(wishlistItem);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<WishlistItem?>> GetWishlistItemByUserIdAsync(Guid userId)
    {
        return await context.WishlistItems.Where(w => w.UserId == userId).ToListAsync();
    }
}