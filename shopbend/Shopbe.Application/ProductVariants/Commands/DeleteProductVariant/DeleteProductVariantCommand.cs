using MediatR;

namespace Shopbe.Application.ProductVariants.Commands.UpdateProductVariant;
public record DeleteProductVariantCommand(Guid Id) : IRequest<bool>;