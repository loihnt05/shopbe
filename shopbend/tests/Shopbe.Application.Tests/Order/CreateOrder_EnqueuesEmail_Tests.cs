using Moq;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.Notifications;
using Shopbe.Application.Order.Commands.CreateOrder;
using Shopbe.Application.Order.Dtos;
using Shopbe.Domain.Entities.ShoppingCart;

namespace Shopbe.Application.Tests.Order;

public class CreateOrder_EnqueuesEmail_Tests
{
    [Fact]
    public async Task Handle_Enqueues_Email_When_User_Has_Email()
    {
        var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var emailQueue = new Mock<IEmailQueue>(MockBehavior.Strict);

        var userId = Guid.NewGuid();
        // order id is generated inside handler

        // Transaction
        uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        uow.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);

        // Cart
        var cartRepo = new Mock<Shopbe.Application.Common.Interfaces.IShoppingCart.ICartRepository>(MockBehavior.Strict);
        cartRepo.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShoppingCart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem>
                {
                    new() { Id = Guid.NewGuid(), ProductVariantId = Guid.NewGuid(), Quantity = 1 }
                }
            });
        cartRepo.Setup(x => x.ClearAsync(userId, It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));

        // Repos required by handler
        var variantRepo = new Mock<Shopbe.Application.Common.Interfaces.IProduct.IProductVariantRepository>(MockBehavior.Strict);
        variantRepo.Setup(x => x.GetProductVariantByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Shopbe.Domain.Entities.Product.ProductVariant
            {
                Id = Guid.NewGuid(),
                Price = 100_000,
                Sku = "SKU",
                Product = new Shopbe.Domain.Entities.Product.Product { Id = Guid.NewGuid(), Name = "Test Product" }
            });

        var ordersRepo = new Mock<Shopbe.Application.Common.Interfaces.IOrder.IOrderRepository>(MockBehavior.Strict);
        ordersRepo.Setup(x => x.AddAsync(It.IsAny<Shopbe.Domain.Entities.Order.Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userRepo = new Mock<Shopbe.Application.Common.Interfaces.IUser.IUserRepository>(MockBehavior.Strict);
        userRepo.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new Shopbe.Domain.Entities.User.User { Id = userId, FullName = "Test", Email = "test@example.com" });

        // Unused repos in this scenario
        var addrRepo = new Mock<Shopbe.Application.Common.Interfaces.IUser.IUserAddressRepository>(MockBehavior.Strict);
        addrRepo.Setup(x => x.GetUserAddressesByUserIdAsync(userId)).ReturnsAsync(new List<Shopbe.Domain.Entities.User.UserAddress>());

        // Setup UoW properties
        uow.SetupGet(x => x.Cart).Returns(cartRepo.Object);
        uow.SetupGet(x => x.ProductVariant).Returns(variantRepo.Object);
        uow.SetupGet(x => x.Orders).Returns(ordersRepo.Object);
        uow.SetupGet(x => x.Users).Returns(userRepo.Object);
        uow.SetupGet(x => x.UserAddresses).Returns(addrRepo.Object);

        // Coupon repo might be accessed if CouponCode is passed; ensure not used

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        uow.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        emailQueue.Setup(x => x.EnqueueAsync(
                It.Is<string>(s => s == "test@example.com"),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.Is<Guid?>(g => g == userId),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var handler = new CreateOrderHandler(uow.Object, emailQueue.Object);

        var cmd = new CreateOrderCommand(userId, new CreateOrderRequestDto
        {
            ShippingReceiverName = "R",
            ShippingPhone = "1",
            ShippingAddressLine = "L",
            ShippingCity = "C",
            ShippingDistrict = "D",
            ShippingWard = "W",
            UseDefaultAddressIfAvailable = false
        });

        await handler.Handle(cmd, CancellationToken.None);

        emailQueue.VerifyAll();
    }
}



