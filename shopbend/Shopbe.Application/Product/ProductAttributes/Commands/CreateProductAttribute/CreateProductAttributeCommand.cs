using MediatR;
using Shopbe.Application.Product.ProductAttributes.Dtos;

namespace Shopbe.Application.Product.ProductAttributes.Commands.CreateProductAttribute;

public record CreateProductAttributeCommand(ProductAttributeRequestDto Request) : IRequest<ProductAttributeResponseDto>;

