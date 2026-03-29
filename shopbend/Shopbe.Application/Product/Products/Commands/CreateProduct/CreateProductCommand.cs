using MediatR;
using Shopbe.Application.Products.Dtos;

namespace Shopbe.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(ProductRequestDto Request) : IRequest<ProductResponseDto>;