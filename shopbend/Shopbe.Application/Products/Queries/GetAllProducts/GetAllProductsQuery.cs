using MediatR;
using Shopbe.Application.Products.Dtos;

namespace Shopbe.Application.Products.Queries.GetAllProducts;

public record GetAllProductsQuery(ProductQueryDto Filter) : IRequest<IEnumerable<ProductResponseDto>>;
