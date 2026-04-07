using MediatR;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Commands.UpdateProductImage;

public record UpdateProductImageCommand(ProductImageRequestDto Request, Guid Id) : IRequest<ProductImageResponseDto>;