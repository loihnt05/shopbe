using MediatR;
using Shopbe.Application.ProductsImages.Dtos;

namespace Shopbe.Application.ProductsImages.Commands.CreateProductImage;

public record CreateProductImageCommand(ProductImageRequestDto Request, Guid Id) : IRequest<ProductImageResponseDto>;