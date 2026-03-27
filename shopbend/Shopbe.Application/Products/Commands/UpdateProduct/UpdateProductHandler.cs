using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Products.Dtos;
using Shopbe.Application.ProductsImages.Dtos;
using Shopbe.Application.ProductVariants.Dtos;
using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Products.Commands.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductResponseDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Product.GetProductByIdAsync(request.Id);
        if (product is null)
            throw new KeyNotFoundException($"Product with id '{request.Id}' was not found.");

        if (string.IsNullOrWhiteSpace(request.Request.Name))
            throw new ArgumentException("Product name is required.");

        if (request.Request.Price < 0)
            throw new ArgumentException("Product price cannot be negative.");

        if (request.Request.StockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.");

        ValidateImages(request.Request.Images);
        ValidateVariants(request.Request.Variants);

        var category = await _unitOfWork.Category.GetCategoryByIdAsync(request.Request.CategoryId);
        if (category is null)
            throw new KeyNotFoundException($"Category with id '{request.Request.CategoryId}' was not found.");

        var images = (request.Request.Images ?? Enumerable.Empty<ProductImageRequestDto>()).ToList();
        var variants = (request.Request.Variants ?? Enumerable.Empty<ProductVariantRequestDto>()).ToList();

        product.Name = request.Request.Name;
        product.Description = request.Request.Description;
        product.Price = request.Request.Price;
        product.ImageUrl = ResolvePrimaryImageUrl(request.Request.ImageUrl, images);
        product.StockQuantity = request.Request.StockQuantity;
        product.CategoryId = request.Request.CategoryId;

        product.Images.Clear();
        foreach (var image in images)
        {
            product.Images.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary,
                ProductId = product.Id
            });
        }

        product.Variants.Clear();
        foreach (var variant in variants)
        {
            product.Variants.Add(new ProductVariant
            {
                Id = Guid.NewGuid(),
                SKU = variant.SKU,
                Price = variant.Price,
                StockQuantity = variant.StockQuantity,
                ImageUrl = variant.ImageUrl,
                ProductId = product.Id
            });
        }

        await _unitOfWork.Product.UpdateProductAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDtoMapper.ToResponse(product);
    }

    private static string ResolvePrimaryImageUrl(string fallbackImageUrl, IEnumerable<ProductImageRequestDto> images)
    {
        var primaryImage = images.FirstOrDefault(i => i.IsPrimary);
        if (primaryImage is not null)
        {
            return primaryImage.ImageUrl;
        }

        var firstImage = images.FirstOrDefault();
        if (firstImage is not null)
        {
            return firstImage.ImageUrl;
        }

        return fallbackImageUrl;
    }

    private static void ValidateImages(IEnumerable<ProductImageRequestDto>? images)
    {
        if (images is null)
        {
            return;
        }

        var imageList = images.ToList();
        if (imageList.Any(i => string.IsNullOrWhiteSpace(i.ImageUrl)))
        {
            throw new ArgumentException("Image URL is required for all product images.");
        }

        if (imageList.Count(i => i.IsPrimary) > 1)
        {
            throw new ArgumentException("Only one product image can be marked as primary.");
        }
    }

    private static void ValidateVariants(IEnumerable<ProductVariantRequestDto>? variants)
    {
        if (variants is null)
        {
            return;
        }

        var variantList = variants.ToList();
        if (variantList.Any(v => string.IsNullOrWhiteSpace(v.SKU)))
        {
            throw new ArgumentException("SKU is required for all variants.");
        }

        if (variantList.Any(v => v.Price < 0))
        {
            throw new ArgumentException("Variant price cannot be negative.");
        }

        if (variantList.Any(v => v.StockQuantity < 0))
        {
            throw new ArgumentException("Variant stock quantity cannot be negative.");
        }

        var hasDuplicateSku = variantList
            .GroupBy(v => v.SKU, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

        if (hasDuplicateSku)
        {
            throw new ArgumentException("Variant SKUs must be unique within a product.");
        }
    }
}
