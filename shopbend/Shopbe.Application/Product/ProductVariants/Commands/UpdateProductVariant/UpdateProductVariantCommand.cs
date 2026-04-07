using MediatR;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Commands.UpdateProductVariant;

public record UpdateProductVariantCommand(ProductVariantRequestDto Request, Guid Id) : IRequest<ProductVariantResponseDto>;