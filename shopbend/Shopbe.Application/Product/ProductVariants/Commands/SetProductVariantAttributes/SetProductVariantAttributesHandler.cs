using MediatR;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Product.ProductVariants.Dtos;
using Shopbe.Domain.Entities.Product;

namespace Shopbe.Application.Product.ProductVariants.Commands.SetProductVariantAttributes;

public class SetProductVariantAttributesHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SetProductVariantAttributesCommand, ProductVariantResponseDto>
{
    public async Task<ProductVariantResponseDto> Handle(SetProductVariantAttributesCommand request, CancellationToken cancellationToken)
    {
        if (request.VariantId == Guid.Empty)
        {
            throw new ArgumentException("VariantId is required.");
        }

        var variant = await unitOfWork.ProductVariant.GetProductVariantByIdAsync(request.VariantId);
        if (variant is null)
        {
            throw new KeyNotFoundException($"Variant with id '{request.VariantId}' was not found.");
        }

        var attributeValueIds = (request.Request.AttributeValueIds ?? Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // Validate AttributeValueIds exist
        foreach (var attributeValueId in attributeValueIds)
        {
            var value = await unitOfWork.AttributeValue.GetValueByIdAsync(attributeValueId);
            if (value is null)
            {
                throw new KeyNotFoundException($"Attribute value with id '{attributeValueId}' was not found.");
            }
        }

        await unitOfWork.ProductVariantAttribute.DeleteByVariantIdAsync(request.VariantId);

        if (attributeValueIds.Count > 0)
        {
            var items = attributeValueIds.Select(id => new ProductVariantAttribute
            {
                VariantId = request.VariantId,
                AttributeValueId = id
            });

            await unitOfWork.ProductVariantAttribute.AddRangeAsync(items);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductVariantResponseDto(
            variant.Id,
            variant.ProductId,
            variant.Sku,
            variant.Price,
            variant.StockQuantity,
            variant.IsActive,
            attributeValueIds);
    }
}

