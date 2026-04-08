using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Product.AttributeValues.Commands.CreateAttributeValue;

public class CreateAttributeValueHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAttributeValueCommand, AttributeValueResponseDto>
{
    public async Task<AttributeValueResponseDto> Handle(CreateAttributeValueCommand request, CancellationToken cancellationToken)
    {
        if (request.AttributeId == Guid.Empty)
        {
            throw new ArgumentException("AttributeId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Request.Value))
        {
            throw new ArgumentException("Value is required.");
        }


        var attribute = await unitOfWork.ProductAttribute.GetAttributeByIdAsync(request.AttributeId);
        if (attribute is null)
        {
            throw new KeyNotFoundException($"Attribute with id '{request.AttributeId}' was not found.");
        }

        var valueText = request.Request.Value.Trim();
        var existing = await unitOfWork.AttributeValue.GetValueAsync(request.AttributeId, valueText);
        if (existing is not null)
        {
            throw new ArgumentException("Attribute value must be unique within the attribute.");
        }

        var value = new AttributeValue
        {
            Id = Guid.NewGuid(),
            AttributeId = request.AttributeId,
            Value = valueText
        };

        await unitOfWork.AttributeValue.AddValueAsync(value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AttributeValueResponseDto(value.Id, value.Value, value.AttributeId);
    }
}

