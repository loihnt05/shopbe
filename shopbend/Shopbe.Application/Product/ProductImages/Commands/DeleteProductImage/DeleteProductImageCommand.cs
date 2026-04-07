using MediatR;

namespace Shopbe.Application.Product.ProductImages.Commands.DeleteProductImage;

public record DeleteProductImageCommand(Guid Id) : IRequest<bool>;