using MediatR;
using Shopbe.Application.Wishlist.Dtos;

namespace Shopbe.Application.Wishlist.Queries.GetMyWishlist;

public sealed record GetMyWishlistQuery(Guid UserId, WishlistQueryDto Query) 
    : IRequest<IReadOnlyList<WishlistItemResponseDto>>;
