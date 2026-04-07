using MediatR;
using Shopbe.Application.Product.ProductAttributes.Dtos;

namespace Shopbe.Application.Product.ProductAttributes.Queries.GetAllProductAttributes;

public record GetAllProductAttributesQuery() : IRequest<IEnumerable<ProductAttributeResponseDto>>;

