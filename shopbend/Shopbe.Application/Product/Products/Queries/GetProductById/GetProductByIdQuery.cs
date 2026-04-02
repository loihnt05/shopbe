using MediatR;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponseDto?>;
