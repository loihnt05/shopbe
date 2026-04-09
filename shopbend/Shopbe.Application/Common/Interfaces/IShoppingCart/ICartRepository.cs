using Shopbe.Domain.Entities.ShoppingCart;

namespace Shopbe.Application.Common.Interfaces.IShoppingCart;

public interface ICartRepository
{
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ShoppingCart> GetOrCreateByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CartItem?> GetItemAsync(Guid cartId, Guid productVariantId, CancellationToken cancellationToken = default);

    Task<CartItem> AddOrIncrementItemAsync(
        Guid userId,
        Guid productVariantId,
        int quantity,
        decimal unitPrice,
        CancellationToken cancellationToken = default);

    Task<CartItem?> SetItemQuantityAsync(
        Guid userId,
        Guid productVariantId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveItemAsync(Guid userId, Guid productVariantId, CancellationToken cancellationToken = default);
    Task<int> ClearAsync(Guid userId, CancellationToken cancellationToken = default);
}