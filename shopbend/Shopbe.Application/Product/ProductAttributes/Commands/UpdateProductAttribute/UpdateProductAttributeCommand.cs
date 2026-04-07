using MediatR;
using Shopbe.Application.Product.ProductAttributes.Dtos;

namespace Shopbe.Application.Product.ProductAttributes.Commands.UpdateProductAttribute;

public record UpdateProductAttributeCommand(Guid Id, ProductAttributeRequestDto Request) : IRequest<ProductAttributeResponseDto>;

