using MediatR;

namespace Shopbe.Application.Product.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
