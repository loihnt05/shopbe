using MediatR;
using Shopbe.Application.Products.Dtos;

namespace Shopbe.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponseDto?>;
