using MediatR;

namespace Shopbe.Application.Wishlist.Commands.RemoveWishlistItem;

public sealed record RemoveWishlistItemCommand(Guid UserId, Guid ProductId) : IRequest<bool>;

