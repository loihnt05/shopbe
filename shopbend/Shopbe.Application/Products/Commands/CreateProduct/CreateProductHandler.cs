using MediatR;
using Shopbe.Application.Interfaces;
using Shopbe.Domain.Entities;

namespace Shopbe.Application.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
	private readonly IProductRepository _productRepository;

	public CreateProductHandler(IProductRepository productRepository)
	{
		_productRepository = productRepository;
	}

	public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
	{
		var product = new Product
		{
			Id = Guid.NewGuid(),
			Name = request.Name,
			Description = request.Description,
			Price = request.Price,
			StockQuantity = request.StockQuantity,
			ImageUrl = request.ImageUrl,
			CategoryId = request.CategoryId
		};

		await _productRepository.AddProductAsync(product);
		return product.Id;
	}
}