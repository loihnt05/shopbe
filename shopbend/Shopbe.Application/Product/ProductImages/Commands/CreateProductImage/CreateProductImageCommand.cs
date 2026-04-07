using MediatR;
using Shopbe.Application.Product.ProductImages.Dtos;

namespace Shopbe.Application.Product.ProductImages.Commands.CreateProductImage;

public record CreateProductImageCommand(ProductImageRequestDto Request, Guid Id) : IRequest<ProductImageResponseDto>;