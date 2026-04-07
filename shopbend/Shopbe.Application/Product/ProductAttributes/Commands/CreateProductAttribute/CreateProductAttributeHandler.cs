using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.AttributeValues.Dtos;
using Shopbe.Application.Product.ProductAttributes.Dtos;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Product.ProductAttributes.Commands.CreateProductAttribute;

public class CreateProductAttributeHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductAttributeCommand, ProductAttributeResponseDto>
{
    public async Task<ProductAttributeResponseDto> Handle(CreateProductAttributeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.Name))
        {
            throw new ArgumentException("Attribute name is required.");
        }

        var name = request.Request.Name.Trim();
        var existing = await unitOfWork.ProductAttribute.GetAttributeByNameAsync(name);
        if (existing is not null)
        {
            throw new ArgumentException("Attribute name must be unique.");
        }

        var attribute = new ProductAttribute
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        await unitOfWork.ProductAttribute.AddAttributeAsync(attribute);
        // Note: repo currently saves internally; also call UoW save for consistency if needed.

        return new ProductAttributeResponseDto(attribute.Id, attribute.Name, Array.Empty<AttributeValueResponseDto>());
    }
}

