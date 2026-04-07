using MediatR;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Queries.GetProductImageById;

public record GetProductImageByIdQuery(Guid Id) : IRequest<ProductImageResponseDto>;