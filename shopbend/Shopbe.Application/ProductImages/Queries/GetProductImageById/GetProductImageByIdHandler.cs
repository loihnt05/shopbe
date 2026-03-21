using MediatR;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Commands.CreateProductImage;

public record CreateProductImageCommand(ProductImageRequestDto request, Guid ProductId) : IRequest<ProductImageResponseDto>;