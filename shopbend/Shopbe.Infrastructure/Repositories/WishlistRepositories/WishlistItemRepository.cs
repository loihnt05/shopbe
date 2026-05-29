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

    public async Task DeleteBulkAsync(Guid userId, IEnumerable<Guid> productIds)
    {
        var itemsToRemove = await context.WishlistItems
            .Where(w => w.UserId == userId && productIds.Contains(w.ProductId))
            .ToListAsync();

        if (itemsToRemove.Count != 0)
        {
            context.WishlistItems.RemoveRange(itemsToRemove);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<WishlistItem?>> GetWishlistItemByUserIdAsync(
        Guid userId,
        string? sortBy = null,
        bool? inStockOnly = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var query = context.WishlistItems
            .Include(w => w.Product)
                .ThenInclude(p => p!.Category)
            .Include(w => w.Product)
                .ThenInclude(p => p!.Brand)
            .Include(w => w.Product)
                .ThenInclude(p => p!.Images)
            .Include(w => w.Product)
                .ThenInclude(p => p!.Variants)
                    .ThenInclude(v => v.ProductVariantAttributes)
                        .ThenInclude(a => a.AttributeValue)
                            .ThenInclude(av => av!.Attribute)
            .Include(w => w.Product)
                .ThenInclude(p => p!.Reviews)
            .Where(w => w.UserId == userId)
            .AsQueryable();

        if (inStockOnly == true)
        {
            query = query.Where(w => w.Product != null && w.Product.Variants.Sum(v => v.StockQuantity) > 0);
        }

        query = sortBy switch
        {
            "PriceAsc" => query.OrderBy(w => w.Product != null ? w.Product.BasePrice : 0),
            "PriceDesc" => query.OrderByDescending(w => w.Product != null ? w.Product.BasePrice : 0),
            "Discount" => query.OrderByDescending(w => w.Product != null && w.Product.DiscountPrice != null 
                ? (w.Product.BasePrice - w.Product.DiscountPrice) / w.Product.BasePrice 
                : 0),
            _ => query.OrderByDescending(w => w.CreatedAt) // Recently added
        };

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
