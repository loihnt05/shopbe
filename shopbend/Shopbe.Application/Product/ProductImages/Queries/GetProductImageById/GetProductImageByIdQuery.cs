using MediatR;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Queries.GetProductImageById;

public record GetProductImageByIdQuery(Guid Id) : IRequest<ProductImageResponseDto>;