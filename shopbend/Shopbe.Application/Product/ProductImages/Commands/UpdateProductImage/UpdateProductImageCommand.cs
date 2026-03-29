using MediatR;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Commands.UpdateProductImage;

public record UpdateProductImageCommand(ProductImageRequestDto Request, Guid Id) : IRequest<ProductImageResponseDto>;