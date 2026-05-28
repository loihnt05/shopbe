using Moq;
using Shopbe.Application.Cart;
using Shopbe.Application.Cart.Commands.ApplyCoupon;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IOrder;
using Shopbe.Application.Common.Interfaces.IShoppingCart;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Domain.Entities.ShoppingCart;
using Shopbe.Domain.Enums;
using Xunit;
using CouponEntity = Shopbe.Domain.Entities.Order.Coupon;

namespace Shopbe.Application.Tests.Cart;

public class ApplyCouponTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ICurrentUser> _currentUser;
    private readonly Mock<ICouponRepository> _couponRepository;
    private readonly Mock<ICartRepository> _cartRepository;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly ApplyCouponHandler _handler;

    public ApplyCouponTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        _couponRepository = new Mock<ICouponRepository>(MockBehavior.Strict);
        _cartRepository = new Mock<ICartRepository>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);

        _unitOfWork.SetupGet(u => u.Coupons).Returns(_couponRepository.Object);
        _unitOfWork.SetupGet(u => u.Cart).Returns(_cartRepository.Object);
        _unitOfWork.SetupGet(u => u.Users).Returns(_userRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new ApplyCouponHandler(_unitOfWork.Object, _currentUser.Object);
    }

    [Fact]
    public async Task Handle_ShouldApplyCoupon_WhenValid()
    {
        // Arrange
        var keycloakId = "user-123";
        var userId = Guid.NewGuid();
        var couponCode = "SAVE10";
        var coupon = new CouponEntity
        {
            Id = Guid.NewGuid(),
            Code = couponCode,
            IsActive = true,
            ExpiredAt = DateTime.UtcNow.AddDays(1),
            MinOrderAmount = 100,
            DiscountType = DiscountType.FixedAmount,
            Value = 10,
            Count = 10
        };
        var cart = new ShoppingCart
        {
            UserId = userId,
            CartItems = new List<CartItem>
            {
                new CartItem { UnitPrice = 150, Quantity = 1 }
            }
        };

        _currentUser.SetupGet(c => c.KeycloakId).Returns(keycloakId);
        _userRepository.Setup(r => r.GetUserByKeycloakIdAsync(keycloakId)).ReturnsAsync(new Shopbe.Domain.Entities.User.User { Id = userId, FullName = "Test User" });
        _cartRepository.Setup(r => r.GetOrCreateByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        _couponRepository.Setup(r => r.GetByCodeAsync(couponCode, It.IsAny<CancellationToken>())).ReturnsAsync(coupon);

        // Act
        var result = await _handler.Handle(new ApplyCouponCommand(couponCode), CancellationToken.None);

        // Assert
        Assert.Equal(couponCode, result.CouponCode);
        Assert.Equal(10, result.DiscountAmount);
        Assert.Equal(140, result.Total);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCouponNotFound()
    {
        // Arrange
        var keycloakId = "user-123";
        var userId = Guid.NewGuid();
        var couponCode = "INVALID";

        _currentUser.SetupGet(c => c.KeycloakId).Returns(keycloakId);
        _userRepository.Setup(r => r.GetUserByKeycloakIdAsync(keycloakId)).ReturnsAsync(new Shopbe.Domain.Entities.User.User { Id = userId, FullName = "Test User" });
        _cartRepository.Setup(r => r.GetOrCreateByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new ShoppingCart { UserId = userId });
        _couponRepository.Setup(r => r.GetByCodeAsync(couponCode, It.IsAny<CancellationToken>())).ReturnsAsync((CouponEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(new ApplyCouponCommand(couponCode), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCouponCountIsZero()
    {
        // Arrange
        var keycloakId = "user-123";
        var userId = Guid.NewGuid();
        var couponCode = "EXHAUSTED";
        var coupon = new CouponEntity
        {
            Id = Guid.NewGuid(),
            Code = couponCode,
            IsActive = true,
            ExpiredAt = DateTime.UtcNow.AddDays(1),
            MinOrderAmount = 100,
            DiscountType = DiscountType.FixedAmount,
            Value = 10,
            Count = 0
        };

        _currentUser.SetupGet(c => c.KeycloakId).Returns(keycloakId);
        _userRepository.Setup(r => r.GetUserByKeycloakIdAsync(keycloakId)).ReturnsAsync(new Shopbe.Domain.Entities.User.User { Id = userId, FullName = "Test User" });
        _cartRepository.Setup(r => r.GetOrCreateByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new ShoppingCart { UserId = userId });
        _couponRepository.Setup(r => r.GetByCodeAsync(couponCode, It.IsAny<CancellationToken>())).ReturnsAsync(coupon);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new ApplyCouponCommand(couponCode), CancellationToken.None));
        Assert.Equal("Coupon usage limit reached.", ex.Message);
    }
}
