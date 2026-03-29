using MediatR;
using Shopbe.Application.ProductVariants.Dtos;

namespace Shopbe.Application.ProductsVariants.Queries.GetAllProducts;
public record GetAllProductVariantQuery(ProductVariantQueryDto Filter) : IRequest<IEnumerable<ProductVariantResponseDto>>;