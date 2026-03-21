using MediatR;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.ProductVariants.Commands.CreateProductVariant;

public record CreateProductVariantCommand(ProductVariantRequestDto request, Guid ProductId) : IRequest<ProductVariantResponseDto>;