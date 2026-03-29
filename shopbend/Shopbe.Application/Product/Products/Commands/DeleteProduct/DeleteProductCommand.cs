using MediatR;

namespace Shopbe.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
