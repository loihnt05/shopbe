using MediatR;

namespace Shopbe.Application.Wishlist.Queries.IsInWishlist;

public sealed record IsInWishlistQuery(Guid UserId, Guid ProductId) : IRequest<bool>;

