using MediatR;
using Shopbe.Application.Product.Products.Dtos;

namespace Shopbe.Application.Product.Products.Commands.CreateProduct;

public record CreateProductCommand(ProductRequestDto Request) : IRequest<ProductResponseDto>;