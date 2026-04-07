using MediatR;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Queries.GetProductVariantById;

public record GetProductVariantByIdQuery(Guid Id) : IRequest<ProductVariantResponseDto>;