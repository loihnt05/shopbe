namespace Shopbe.Application.Wishlist.Dtos;

public sealed record WishlistItemResponseDto(
    Guid Id,
    Guid UserId,
    Guid ProductId);

