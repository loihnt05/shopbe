using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Application.Products.Dtos;
using Shopbe.Domain.Entities;

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

		var category = await _unitOfWork.Category.GetCategoryByIdAsync(command.Request.CategoryId);
		if (category is null)
		{
			throw new KeyNotFoundException($"Category with id '{command.Request.CategoryId}' was not found.");
		}

		var product = new Product
		{
			Id = Guid.NewGuid(),
			Name = command.Request.Name,
			Description = command.Request.Description,
			Price = command.Request.Price,
			StockQuantity = command.Request.StockQuantity,
			ImageUrl = command.Request.ImageUrl,
			CategoryId = command.Request.CategoryId
		};

		await _unitOfWork.Product.AddProductAsync(product);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return new ProductResponseDto(
			product.Id,
			product.Name,
			product.Description,
			product.Price,
			product.ImageUrl,
			product.StockQuantity,
			product.CategoryId
		);
	}
}