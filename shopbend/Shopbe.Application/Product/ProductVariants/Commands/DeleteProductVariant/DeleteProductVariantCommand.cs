using MediatR;

namespace Shopbe.Application.Product.ProductVariants.Commands.DeleteProductVariant;
public record DeleteProductVariantCommand(Guid Id) : IRequest<bool>;