using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;
using Shopbe.Application.Product.ProductAttributes.Dtos;

namespace Shopbe.Application.Product.ProductAttributes.Queries.GetProductAttributeById;

public class GetProductAttributeByIdHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetProductAttributeByIdQuery, ProductAttributeResponseDto>
{
    public async Task<ProductAttributeResponseDto> Handle(GetProductAttributeByIdQuery request, CancellationToken cancellationToken)
    {
        var attribute = await unitOfWork.ProductAttribute.GetAttributeByIdAsync(request.Id);
        if (attribute is null)
        {
            throw new KeyNotFoundException($"Attribute with id '{request.Id}' was not found.");
        }

        var values = attribute.AttributeValues
            .Select(v => new AttributeValueResponseDto(v.Id, v.Value, v.AttributeId))
            .ToArray();

        return new ProductAttributeResponseDto(attribute.Id, attribute.Name, values);
    }
}

