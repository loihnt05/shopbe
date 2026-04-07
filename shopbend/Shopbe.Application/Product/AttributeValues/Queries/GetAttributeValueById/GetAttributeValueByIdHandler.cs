using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Queries.GetAttributeValueById;

public class GetAttributeValueByIdHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAttributeValueByIdQuery, AttributeValueResponseDto>
{
    public async Task<AttributeValueResponseDto> Handle(GetAttributeValueByIdQuery request, CancellationToken cancellationToken)
    {
        var value = await unitOfWork.AttributeValue.GetValueByIdAsync(request.Id);
        if (value is null)
        {
            throw new KeyNotFoundException($"Attribute value with id '{request.Id}' was not found.");
        }

        return new AttributeValueResponseDto(value.Id, value.Value, value.AttributeId);
    }
}

