using MediatR;
using Shopbe.Application.Product.ProductAttributes.Dtos;

namespace Shopbe.Application.Product.ProductAttributes.Queries.GetProductAttributeById;

public record GetProductAttributeByIdQuery(Guid Id) : IRequest<ProductAttributeResponseDto>;

