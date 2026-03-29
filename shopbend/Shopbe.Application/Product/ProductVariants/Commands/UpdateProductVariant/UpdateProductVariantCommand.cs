using MediatR;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.ProductVariants.Commands.UpdateProductVariant;

public record UpdateProductVariantCommand(ProductVariantRequestDto request, Guid Id) : IRequest<ProductVariantResponseDto>;