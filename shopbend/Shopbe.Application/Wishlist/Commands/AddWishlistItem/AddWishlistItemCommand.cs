using MediatR;
using Shopbe.Application.Wishlist.Dtos;

namespace Shopbe.Application.Wishlist.Commands.AddWishlistItem;

public sealed record AddWishlistItemCommand(Guid UserId, AddWishlistItemRequestDto Request) : IRequest<WishlistItemResponseDto>;

