using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;
using Shopbe.Application.Product.ProductAttributes.Dtos;

namespace Shopbe.Application.Product.ProductAttributes.Queries.GetAllProductAttributes;

public class GetAllProductAttributesHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllProductAttributesQuery, IEnumerable<ProductAttributeResponseDto>>
{
    public async Task<IEnumerable<ProductAttributeResponseDto>> Handle(GetAllProductAttributesQuery request, CancellationToken cancellationToken)
    {
        var attributes = await unitOfWork.ProductAttribute.GetAllAttributesAsync();
        return attributes
            .Select(a => new ProductAttributeResponseDto(
                a.Id,
                a.Name,
                a.AttributeValues
                    .OrderBy(v => v.Value)
                    .Select(v => new AttributeValueResponseDto(v.Id, v.Value, v.AttributeId))
                    .ToArray()))
            .ToList();
    }
}

