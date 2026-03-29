using MediatR;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.ProductsVariants.Queries.GetProductVariantById;

public record GetProductVariantByIdQuery(Guid Id) : IRequest<ProductVariantResponseDto>;