namespace Shopbe.Application.Wishlist.Dtos;

public record WishlistQueryDto(
    string? SortBy,
    bool? InStockOnly,
    int PageNumber = 1,
    int PageSize = 20
);
