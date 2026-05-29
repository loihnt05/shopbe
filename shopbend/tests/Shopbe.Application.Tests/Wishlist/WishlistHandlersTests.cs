using Moq;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IWishlist;
using Shopbe.Application.Wishlist.Dtos;
using Shopbe.Application.Wishlist.Queries.GetMyWishlist;
using Shopbe.Domain.Entities.Wishlist;
using DomainProduct = Shopbe.Domain.Entities.Product.Product;

namespace Shopbe.Application.Tests.Wishlist;

public class WishlistHandlersTests
{
    [Fact]
    public async Task GetMyWishlistHandler_ShouldReturnMappedDtos()
    {
        // Arrange
        var wishlistRepository = new Mock<IWishlistItemRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(u => u.WishlistItems).Returns(wishlistRepository.Object);

        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var items = new List<WishlistItem>
        {
            new WishlistItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
                Product = new DomainProduct
                {
                    Id = productId,
                    Name = "Test Product",
                    Slug = "test-product",
                    BasePrice = 100,
                    IsActive = true
                }
            }
        };

        wishlistRepository
            .Setup(r => r.GetWishlistItemByUserIdAsync(userId, null, null, 1, 20))
            .ReturnsAsync(items);

        var handler = new GetMyWishlistHandler(unitOfWork.Object);
        var query = new GetMyWishlistQuery(userId, new WishlistQueryDto(null, null, 1, 20));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Product", result[0].Name);
        Assert.Equal(100, result[0].Price);
    }
}
