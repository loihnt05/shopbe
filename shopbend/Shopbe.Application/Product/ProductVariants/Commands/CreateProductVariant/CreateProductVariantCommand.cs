using MediatR;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Commands.CreateProductVariant;

public record CreateProductVariantCommand(ProductVariantRequestDto Request, Guid ProductId) : IRequest<ProductVariantResponseDto>;