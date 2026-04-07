using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Queries.GetAttributeValuesByAttributeId;

public class GetAttributeValuesByAttributeIdHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAttributeValuesByAttributeIdQuery, IEnumerable<AttributeValueResponseDto>>
{
    public async Task<IEnumerable<AttributeValueResponseDto>> Handle(GetAttributeValuesByAttributeIdQuery request, CancellationToken cancellationToken)
    {
        var attribute = await unitOfWork.ProductAttribute.GetAttributeByIdAsync(request.AttributeId);
        if (attribute is null)
        {
            throw new KeyNotFoundException($"Attribute with id '{request.AttributeId}' was not found.");
        }

        var values = await unitOfWork.AttributeValue.GetValuesByAttributeIdAsync(request.AttributeId);
        return values
            .Select(v => new AttributeValueResponseDto(v.Id, v.Value, v.AttributeId))
            .ToList();
    }
}

