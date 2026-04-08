using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Queries.GetAllAttributeValues;

public class GetAllAttributeValuesHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAttributeValuesQuery, IEnumerable<AttributeValueResponseDto>>
{
    public async Task<IEnumerable<AttributeValueResponseDto>> Handle(GetAllAttributeValuesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Product.AttributeValue> values;
        
        if (request.AttributeId.HasValue && request.AttributeId.Value != Guid.Empty)
        {
            values = await unitOfWork.AttributeValue.GetValuesByAttributeIdAsync(request.AttributeId.Value);
        }
        else
        {
            values = await unitOfWork.AttributeValue.GetAllAsync();
        }

        return values.Select(v => new AttributeValueResponseDto(v.Id, v.Value, v.AttributeId));
    }
}