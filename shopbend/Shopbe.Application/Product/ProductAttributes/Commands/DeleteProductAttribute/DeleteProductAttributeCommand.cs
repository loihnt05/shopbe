using MediatR;

namespace Shopbe.Application.Product.ProductAttributes.Commands.DeleteProductAttribute;

public record DeleteProductAttributeCommand(Guid Id) : IRequest<bool>;

