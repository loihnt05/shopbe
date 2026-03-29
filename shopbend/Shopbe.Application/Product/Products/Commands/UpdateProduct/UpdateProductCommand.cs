using MediatR;
using Shopbe.Application.Products.Dtos;

namespace Shopbe.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, ProductRequestDto Request) : IRequest<ProductResponseDto>;
