using MediatR;

namespace Shopbe.Application.Wishlist.Commands.BulkRemoveWishlistItems;

public sealed record BulkRemoveWishlistItemsCommand(Guid UserId, IEnumerable<Guid> ProductIds) : IRequest<bool>;
