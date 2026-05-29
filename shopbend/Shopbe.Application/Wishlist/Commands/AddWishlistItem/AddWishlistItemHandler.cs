using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Wishlist.Dtos;
using Shopbe.Domain.Entities.Wishlist;

namespace Shopbe.Application.Wishlist.Commands.AddWishlistItem;

public sealed class AddWishlistItemHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AddWishlistItemCommand, WishlistItemResponseDto>
{
    public async Task<WishlistItemResponseDto> Handle(AddWishlistItemCommand request,
        CancellationToken cancellationToken)
    {
        // idempotent add (avoid duplicates)
        var items = await unitOfWork.WishlistItems.GetWishlistItemByUserIdAsync(request.UserId);
        var existing = items.FirstOrDefault(i => i is not null && i.ProductId == request.Request.ProductId);

        if (existing is not null)
            return existing.ToDto();

        var item = new WishlistItem
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ProductId = request.Request.ProductId
        };

        await unitOfWork.WishlistItems.CreateWishListItemAsync(item);
        
        // Load product for the response
        var product = await unitOfWork.Product.GetProductByIdAsync(request.Request.ProductId);
        item.Product = product;
        
        return item.ToDto();
    }
}

