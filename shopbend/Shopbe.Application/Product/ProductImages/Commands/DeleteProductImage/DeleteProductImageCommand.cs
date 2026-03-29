using MediatR;

namespace Shopbe.Application.ProductsImages.Commands.DeleteProductImage;

public record DeleteProductImageCommand(Guid Id) : IRequest<bool>;