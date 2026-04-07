using MediatR;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Queries.GetAllProductImage;

public record GetAllProductImageQuery(ProductImageQueryDto Filter) : IRequest<IEnumerable<ProductImageResponseDto>>;