using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductImages.Dtos;
using Shopbe.Application.Product.Products.Dtos;
using Shopbe.Application.Product.ProductVariants.Dtos;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Product.Products.Commands.CreateProduct;

public class CreateProductHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, ProductResponseDto>
{
	public async Task<ProductResponseDto> Handle(CreateProductCommand command, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(command.Request.Name))
		{
			throw new ArgumentException("Product name is required.");
		}

		if (command.Request.BasePrice < 0)
		{
			throw new ArgumentException("Product price cannot be negative.");
		}

		ValidateImages(command.Request.Images);
		ValidateVariants(command.Request.Variants);

		var category = await unitOfWork.Category.GetCategoryByIdAsync(command.Request.CategoryId);
		if (category is null)
		{
			throw new KeyNotFoundException($"Category with id '{command.Request.CategoryId}' was not found.");
		}

		var images = (command.Request.Images ?? Enumerable.Empty<ProductImageRequestDto>()).ToList();
		var variants = (command.Request.Variants ?? Enumerable.Empty<ProductVariantRequestDto>()).ToList();

		var product = new Shopbe.Domain.Entities.Product.Product
		{
			Id = Guid.NewGuid(),
			Name = command.Request.Name,
			Description = command.Request.Description,
			BasePrice = command.Request.BasePrice,
			CategoryId = command.Request.CategoryId,
			BrandId = command.Request.BrandId,
			IsActive = command.Request.IsActive,
			// Optional: set navigation property when available
			Category = category
		};

		product.Images = images.Select(image => new ProductImage
		{
			Id = Guid.NewGuid(),
			ProductId = product.Id,
			ImageUrl = image.ImageUrl,
			IsPrimary = image.IsPrimary
		}).ToList();

		product.Variants = variants.Select(variant => new ProductVariant
		{
			Id = Guid.NewGuid(),
			ProductId = product.Id,
			Sku = variant.SKU,
			Price = variant.Price,
			StockQuantity = variant.StockQuantity,
			IsActive = true,
		}).ToList();

		await unitOfWork.Product.AddProductAsync(product);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return ProductDtoMapper.ToResponse(product);
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