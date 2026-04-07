using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Queries.GetAllAttributeValues;

public class GetAllAttributeValuesHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAttributeValuesQuery, IEnumerable<AttributeValueResponseDto>>
{
    public async Task<IEnumerable<AttributeValueResponseDto>> Handle(GetAllAttributeValuesQuery request, CancellationToken cancellationToken)
    {
        if (request.AttributeId == Guid.Empty)
        {
            throw new ArgumentException("AttributeId is required to list attribute values. Use /api/attributes/{attributeId}/values.");
        }

        var values = await unitOfWork.AttributeValue.GetValuesByAttributeIdAsync(request.AttributeId);
        return values.Select(v => new AttributeValueResponseDto(v.Id, v.Value, v.AttributeId));
    }
}