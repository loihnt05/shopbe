using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Products.Dtos;
using Shopbe.Application.ProductsImages.Dtos;
using Shopbe.Application.ProductVariants.Dtos;
using Shopbe.Domain.Entities;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductResponseDto>
{
	private readonly IUnitOfWork _unitOfWork;

	public CreateProductHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<ProductResponseDto> Handle(CreateProductCommand command, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(command.Request.Name))
		{
			throw new ArgumentException("Product name is required.");
		}

		if (command.Request.Price < 0)
		{
			throw new ArgumentException("Product price cannot be negative.");
		}

		if (command.Request.StockQuantity < 0)
		{
			throw new ArgumentException("Stock quantity cannot be negative.");
		}

		ValidateImages(command.Request.Images);
		ValidateVariants(command.Request.Variants);

		var category = await _unitOfWork.Category.GetCategoryByIdAsync(command.Request.CategoryId);
		if (category is null)
		{
			throw new KeyNotFoundException($"Category with id '{command.Request.CategoryId}' was not found.");
		}

		var images = (command.Request.Images ?? Enumerable.Empty<ProductImageRequestDto>()).ToList();
		var variants = (command.Request.Variants ?? Enumerable.Empty<ProductVariantRequestDto>()).ToList();

		var product = new Product
		{
			Id = Guid.NewGuid(),
			Name = command.Request.Name,
			Description = command.Request.Description,
			BasePrice = command.Request.Price,
			IsActive = true,
			Price = command.Request.Price,
			StockQuantity = command.Request.StockQuantity,
			ImageUrl = ResolvePrimaryImageUrl(command.Request.ImageUrl, images),
		{
			Id = Guid.NewGuid(),
			ImageUrl = image.ImageUrl,
			IsPrimary = image.IsPrimary,
			ProductId = product.Id
		}).ToList();

		product.Variants = variants.Select(variant => new ProductVariant
		{
			Id = Guid.NewGuid(),
			Sku = variant.SKU,
			SKU = variant.SKU,
			StockQuantity = variant.StockQuantity,
			IsActive = true,
			ImageUrl = variant.ImageUrl,
		}).ToList();

		await _unitOfWork.Product.AddProductAsync(product);
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