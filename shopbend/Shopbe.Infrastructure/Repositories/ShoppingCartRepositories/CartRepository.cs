using Microsoft.EntityFrameworkCore;
using Shopbe.Application.Common.Interfaces.IShoppingCart;
using Shopbe.Domain.Entities.ShoppingCart;
using Shopbe.Infrastructure.Persistence;

namespace Shopbe.Infrastructure.Repositories.ShoppingCartRepositories;

public sealed class CartRepository(ShopDbContext context) : ICartRepository
{
    public async Task<ShoppingCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.ShoppingCarts
            .Include(c => c.CartItems)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<ShoppingCart> GetOrCreateByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByUserIdAsync(userId, cancellationToken);
        if (cart is not null) return cart;

        cart = new ShoppingCart { UserId = userId };
        await context.ShoppingCarts.AddAsync(cart, cancellationToken);
        return cart;
    }

    public async Task<CartItem?> GetItemAsync(Guid cartId, Guid productVariantId, CancellationToken cancellationToken = default)
    {
        return await context.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductVariantId == productVariantId, cancellationToken);
    }

    public async Task<CartItem> AddOrIncrementItemAsync(
        Guid userId,
        Guid productVariantId,
        int quantity,
        decimal unitPrice,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be > 0");

        var cart = await GetOrCreateByUserIdAsync(userId, cancellationToken);

        // Ensure cart has an Id for item foreign key.
        if (cart.Id == Guid.Empty)
        {
            // BaseEntity likely sets Guid in constructor; if not, ensure.
            cart.Id = Guid.NewGuid();
        }

        var existing = await GetItemAsync(cart.Id, productVariantId, cancellationToken);
        if (existing is null)
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = productVariantId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                AddedAt = DateTime.UtcNow
            };

            await context.CartItems.AddAsync(item, cancellationToken);
            return item;
        }

        existing.Quantity += quantity;
        // Keep last known unit price up to date
        existing.UnitPrice = unitPrice;
        return existing;
    }

    public async Task<CartItem?> SetItemQuantityAsync(
        Guid userId,
        Guid productVariantId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be > 0");

        var cart = await GetByUserIdAsync(userId, cancellationToken);
        if (cart is null) return null;

        var item = await GetItemAsync(cart.Id, productVariantId, cancellationToken);
        if (item is null) return null;

        item.Quantity = quantity;
        return item;
    }

    public async Task<bool> RemoveItemAsync(Guid userId, Guid productVariantId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByUserIdAsync(userId, cancellationToken);
        if (cart is null) return false;

        var item = await GetItemAsync(cart.Id, productVariantId, cancellationToken);
        if (item is null) return false;

        context.CartItems.Remove(item);
        return true;
    }

    public async Task<int> ClearAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetByUserIdAsync(userId, cancellationToken);
        if (cart is null) return 0;

        // CartItems navigation is included by GetByUserIdAsync
        var count = cart.CartItems.Count;
        if (count == 0) return 0;

        context.CartItems.RemoveRange(cart.CartItems);
        return count;
    }
}

