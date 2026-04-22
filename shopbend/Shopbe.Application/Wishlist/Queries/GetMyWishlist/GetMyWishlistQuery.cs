using MediatR;
using Shopbe.Application.Wishlist.Dtos;

namespace Shopbe.Application.Wishlist.Queries.GetMyWishlist;

public sealed record GetMyWishlistQuery(Guid UserId) : IRequest<IReadOnlyList<WishlistItemResponseDto>>;

