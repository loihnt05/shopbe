using MediatR;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, ProductRequestDto Request) : IRequest<ProductResponseDto>;
