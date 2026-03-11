using MediatR;

namespace Shopbe.Application.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<Guid>
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public int StockQuantity { get; set; }
	public string ImageUrl { get; set; } = string.Empty;
	public Guid CategoryId { get; set; }
}
