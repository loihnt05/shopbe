using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;

namespace Shopbe.Application.Product.AttributeValues.Commands.UpdateAttributeValue;

public class UpdateAttributeValueHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAttributeValueCommand, AttributeValueResponseDto>
{
    public async Task<AttributeValueResponseDto> Handle(UpdateAttributeValueCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Attribute value id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Request.Value))
        {
            throw new ArgumentException("Value is required.");
        }

        var value = await unitOfWork.AttributeValue.GetValueByIdAsync(request.Id);
        if (value is null)
        {
            throw new KeyNotFoundException($"Attribute value with id '{request.Id}' was not found.");
        }

        var valueText = request.Request.Value.Trim();
        var existing = await unitOfWork.AttributeValue.GetValueAsync(value.AttributeId, valueText);
        if (existing is not null && existing.Id != value.Id)
        {
            throw new ArgumentException("Attribute value must be unique within the attribute.");
        }

        value.Value = valueText;
        await unitOfWork.AttributeValue.UpdateValueAsync(value);

        return new AttributeValueResponseDto(value.Id, value.Value, value.AttributeId);
    }
}

