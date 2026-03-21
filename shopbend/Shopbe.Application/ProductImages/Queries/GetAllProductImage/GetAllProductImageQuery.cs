using MediatR;
using Shopbe.Application.ProductsImages.Dtos;
using Shopbe.Domain.Entities;

namespace Shopbe.Application.ProductsImages.Queries.GetAllProductImage;

public record GetAllProductImageQuery(ProductImageQueryDto Filter) : IRequest<IEnumerable<ProductImageResponseDto>>;