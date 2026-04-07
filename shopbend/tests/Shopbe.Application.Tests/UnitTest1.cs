using Moq;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IBrand;
using Shopbe.Application.Common.Interfaces.ICategory;
using Shopbe.Application.Common.Interfaces.IProduct;
using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.Products.Commands.CreateProduct;
using Shopbe.Application.Product.Products.Commands.DeleteProduct;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;
using DomainCategory = Shopbe.Domain.Entities.Category.Category;
using DomainProduct = Shopbe.Domain.Entities.Product.Product;

namespace Shopbe.Application.Tests;

public class ProductHandlersTests
{
    [Fact]
    public async Task CreateProductHandler_ShouldThrow_WhenNameIsMissing()
    {
        var (_, _, _, _, unitOfWork) = CreateUnitOfWorkMocks();
        var handler = new CreateProductHandler(unitOfWork.Object);

        var command = new CreateProductCommand(new ProductRequestDto(
            Name: "   ",
            Description: "Any",
            Price: 10,
            ImageUrl: "fallback.jpg",
            StockQuantity: 2,
            CategoryId: Guid.NewGuid(),
            Images: null,
            Variants: null
        ));

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateProductHandler_ShouldThrow_WhenVariantSkusAreDuplicated()
    {
        var (_, _, _, _, unitOfWork) = CreateUnitOfWorkMocks();
        var handler = new CreateProductHandler(unitOfWork.Object);

        var command = new CreateProductCommand(new ProductRequestDto(
            Name: "T-Shirt",
            Description: "Basic tee",
            Price: 20,
            ImageUrl: "fallback.jpg",
            StockQuantity: 5,
            CategoryId: Guid.NewGuid(),
            Images: null,
            Variants: new[]
            {
                new ProductVariantRequestDto("SKU-001", 20, 2, true, null),
                new ProductVariantRequestDto("sku-001", 21, 3, true, null)
            }
        ));

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateProductHandler_ShouldPersistProductAndUsePrimaryImage()
    {
        var (productRepository, categoryRepository, _, _, unitOfWork) = CreateUnitOfWorkMocks();
        var handler = new CreateProductHandler(unitOfWork.Object);

        var categoryId = Guid.NewGuid();
        categoryRepository
            .Setup(r => r.GetCategoryByIdAsync(categoryId))
            .ReturnsAsync(new DomainCategory { Id = categoryId, Name = "Clothes", Slug = "clothes" });

        DomainProduct? capturedProduct = null;
        productRepository
            .Setup(r => r.AddProductAsync(It.IsAny<DomainProduct>()))
            .Callback<DomainProduct>(p => capturedProduct = p)
            .Returns(Task.CompletedTask);

        var command = new CreateProductCommand(new ProductRequestDto(
            Name: "Sneakers",
            Description: "Running shoes",
            Price: 99.99m,
            ImageUrl: "fallback.jpg",
            StockQuantity: 10,
            CategoryId: categoryId,
            Images: new[]
            {
                new ProductImageRequestDto("image-1.jpg", false),
                new ProductImageRequestDto("image-primary.jpg", true)
            },
            Variants: new[]
            {
                new ProductVariantRequestDto("SNK-42", 99.99m, 5, true, null)
            }
        ));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(capturedProduct);
        Assert.Contains(capturedProduct!.Images, i => i.IsPrimary && i.ImageUrl == "image-primary.jpg");
        Assert.Single(capturedProduct.Variants);
        Assert.Equal(2, capturedProduct.Images.Count);
        Assert.Equal("image-primary.jpg", result.ImageUrl);

        productRepository.Verify(r => r.AddProductAsync(It.IsAny<DomainProduct>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductHandler_ShouldThrow_WhenProductNotFound()
    {
        var (productRepository, _, _, _, unitOfWork) = CreateUnitOfWorkMocks();
        var handler = new DeleteProductHandler(unitOfWork.Object);

        var productId = Guid.NewGuid();
        productRepository
            .Setup(r => r.GetProductByIdAsync(productId))
            .ReturnsAsync((DomainProduct?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new DeleteProductCommand(productId), CancellationToken.None));

        productRepository.Verify(r => r.DeleteProductAsync(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductHandler_ShouldDeleteAndSave_WhenProductExists()
    {
        var (productRepository, _, _, _, unitOfWork) = CreateUnitOfWorkMocks();
        var handler = new DeleteProductHandler(unitOfWork.Object);

        var productId = Guid.NewGuid();
        productRepository
            .Setup(r => r.GetProductByIdAsync(productId))
            .ReturnsAsync(new DomainProduct { Id = productId, Name = "Phone" });
        productRepository
            .Setup(r => r.DeleteProductAsync(productId))
            .Returns(Task.CompletedTask);

        var deleted = await handler.Handle(new DeleteProductCommand(productId), CancellationToken.None);

        Assert.True(deleted);
        productRepository.Verify(r => r.DeleteProductAsync(productId), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static (
        Mock<IProductRepository> ProductRepository,
        Mock<ICategoryRepository> CategoryRepository,
        Mock<IProductImageRepository> ProductImageRepository,
        Mock<IProductVariantRepository> ProductVariantRepository,
        Mock<IUnitOfWork> UnitOfWork) CreateUnitOfWorkMocks()
    {
        var productRepository = new Mock<IProductRepository>(MockBehavior.Strict);
        var categoryRepository = new Mock<ICategoryRepository>(MockBehavior.Strict);
        var brandRepository = new Mock<IBrandRepository>(MockBehavior.Strict);
        var productImageRepository = new Mock<IProductImageRepository>(MockBehavior.Strict);
        var productVariantRepository = new Mock<IProductVariantRepository>(MockBehavior.Strict);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

        unitOfWork.SetupGet(u => u.Product).Returns(productRepository.Object);
        unitOfWork.SetupGet(u => u.Category).Returns(categoryRepository.Object);
        unitOfWork.SetupGet(u => u.Brand).Returns(brandRepository.Object);
        unitOfWork.SetupGet(u => u.ProductImage).Returns(productImageRepository.Object);
        unitOfWork.SetupGet(u => u.ProductVariant).Returns(productVariantRepository.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (productRepository, categoryRepository, productImageRepository, productVariantRepository, unitOfWork);
    }
}