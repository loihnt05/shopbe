using MediatR;
using Shopbe.Application.Product.ProductVariants.Dtos;

namespace Shopbe.Application.Product.ProductVariants.Queries.GetAllProductVariant;
public record GetAllProductVariantQuery(ProductVariantQueryDto Filter) : IRequest<IEnumerable<ProductVariantResponseDto>>;