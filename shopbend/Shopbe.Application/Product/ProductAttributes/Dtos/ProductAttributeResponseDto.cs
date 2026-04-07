using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.ProductAttributes.Dtos;

public record ProductAttributeResponseDto(Guid Id, string Name, AttributeValueResponseDto[] Values);