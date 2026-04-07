using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;
using Shopbe.Application.Product.ProductAttributes.Dtos;

namespace Shopbe.Application.Product.ProductAttributes.Commands.UpdateProductAttribute;

public class UpdateProductAttributeHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductAttributeCommand, ProductAttributeResponseDto>
{
    public async Task<ProductAttributeResponseDto> Handle(UpdateProductAttributeCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Attribute id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Request.Name))
        {
            throw new ArgumentException("Attribute name is required.");
        }

        var attribute = await unitOfWork.ProductAttribute.GetAttributeByIdAsync(request.Id);
        if (attribute is null)
        {
            throw new KeyNotFoundException($"Attribute with id '{request.Id}' was not found.");
        }

        var name = request.Request.Name.Trim();
        var byName = await unitOfWork.ProductAttribute.GetAttributeByNameAsync(name);
        if (byName is not null && byName.Id != attribute.Id)
        {
            throw new ArgumentException("Attribute name must be unique.");
        }

        attribute.Name = name;
        await unitOfWork.ProductAttribute.UpdateAttributeAsync(attribute);

        var values = attribute.AttributeValues
            .Select(v => new AttributeValueResponseDto(v.Id, v.Value, v.AttributeId))
            .ToArray();

        return new ProductAttributeResponseDto(attribute.Id, attribute.Name, values);
    }
}

