using MediatR;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Commands.UpdateProductImage;

public record UpdateProductImageCommand(ProductImageRequestDto request, Guid Id) : IRequest<ProductImageResponseDto>;